import * as cdk from "aws-cdk-lib";
import { Duration } from "aws-cdk-lib";
import * as s3 from "aws-cdk-lib/aws-s3";
import * as s3deploy from "aws-cdk-lib/aws-s3-deployment";
import * as cloudfront from "aws-cdk-lib/aws-cloudfront";
import * as origins from "aws-cdk-lib/aws-cloudfront-origins";
import * as cognito from "aws-cdk-lib/aws-cognito";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import { Construct } from "constructs";

export class InfraStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // 1) Private S3 bucket for frontend
    const siteBucket = new s3.Bucket(this, "NotesFrontendBucket", {
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      encryption: s3.BucketEncryption.S3_MANAGED,
      enforceSSL: true,
      removalPolicy: cdk.RemovalPolicy.DESTROY, // change for prod
      autoDeleteObjects: true, // change for prod
      bucketName: `${this.account}-frontend-bucket`,
    });

    // 2) CloudFront Distribution with S3BucketOrigin (uses OriginAccessIdentity)
    const oai = new cloudfront.OriginAccessIdentity(this, "SiteOAI", {
      comment: "OAI for Notes CloudFront",
    });
    // grant CloudFront OAI read access to bucket
    siteBucket.grantRead(oai);

    const distribution = new cloudfront.Distribution(this, "NotesDistribution", {
      defaultRootObject: "index.html",
      defaultBehavior: {
         origin: origins.S3BucketOrigin.withOriginAccessControl(siteBucket),
        viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,
      },
      errorResponses: [
        {
          httpStatus: 404,
          responseHttpStatus: 200,
          responsePagePath: "/index.html",
          ttl: Duration.minutes(5),
        },
      ],
    });

    // 3) Cognito User Pool + App Client
    const userPool = new cognito.UserPool(this, "NotesUserPool", {
      selfSignUpEnabled: true,
      signInAliases: { email: true },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    const userPoolClient = new cognito.UserPoolClient(this, "NotesUserPoolClient", {
      userPool,
      generateSecret: false,
      authFlows: {
        userPassword: true,
        userSrp: true,
      },
    });

    // 4) DynamoDB table for notes
    const notesTable = new dynamodb.Table(this, "NotesTable", {
      partitionKey: { name: "userId", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "noteId", type: dynamodb.AttributeType.STRING },
      billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
      removalPolicy: cdk.RemovalPolicy.DESTROY, // change for prod
    });

    // 5) C# Lambda function
    // NOTE: you must run `dotnet publish` into this folder before `cdk deploy`
    const notesLambda = new lambda.Function(this, "NotesFunction", {
      runtime: lambda.Runtime.DOTNET_8,
      handler: "MyNote::MyNote.Function::FunctionHandler",
      code: lambda.Code.fromAsset("../lambda/MyNote/MyNote/src/MyNote/bin/Release/net8.0/MyNote.zip"),
      memorySize: 256,
      timeout: Duration.seconds(10),
      environment: {
        TABLE_NAME: notesTable.tableName,
        USER_POOL_ID: userPool.userPoolId, // optional for Lambda usage
      },
    });

    // Grant Lambda permissions to read/write the table
    notesTable.grantReadWriteData(notesLambda);

    // 6) API Gateway (REST) + Cognito Authorizer
    const api = new apigateway.RestApi(this, "NotesApi", {
      restApiName: "Notes Service",
      defaultCorsPreflightOptions: {
        allowOrigins: ["*"], // tighten in prod
        allowMethods: ["OPTIONS", "GET", "PUT", "POST", "DELETE"],
      },
      deployOptions: {
        stageName: "prod",
      },
    });

    // Cognito Authorizer for RestApi
    const authorizer = new apigateway.CognitoUserPoolsAuthorizer(this, "CognitoAuthorizer", {
      cognitoUserPools: [userPool],
    });

    // Create resource /notes
    const notesResource = api.root.addResource("notes");
    const noteResource = notesResource.addResource("{noteId}");

    // Lambda integration
    const lambdaIntegration = new apigateway.LambdaIntegration(notesLambda);

    // Methods with authorizer (protected endpoints)
    notesResource.addMethod("POST", lambdaIntegration, {
      authorizer,
      authorizationType: apigateway.AuthorizationType.COGNITO,
    });
    notesResource.addMethod("GET", lambdaIntegration, {
      authorizer,
      authorizationType: apigateway.AuthorizationType.COGNITO,
    });

    // Single-note CRUD via /notes/{noteId}
    noteResource.addMethod("GET", lambdaIntegration, {
      authorizer,
      authorizationType: apigateway.AuthorizationType.COGNITO,
    });
    noteResource.addMethod("PUT", lambdaIntegration, {
      authorizer,
      authorizationType: apigateway.AuthorizationType.COGNITO,
    });
    noteResource.addMethod("DELETE", lambdaIntegration, {
      authorizer,
      authorizationType: apigateway.AuthorizationType.COGNITO,
    });

    // 7) Deploy frontend to S3 and invalidate CloudFront
    new s3deploy.BucketDeployment(this, "DeployFrontend", {
      sources: [s3deploy.Source.asset("../frontend/dist")],
      destinationBucket: siteBucket,
      distribution,
      distributionPaths: ["/*"],
    });

    // Outputs
    new cdk.CfnOutput(this, "CloudFrontDomain", { value: distribution.domainName });
    new cdk.CfnOutput(this, "ApiUrl", { value: api.url });
    new cdk.CfnOutput(this, "UserPoolId", { value: userPool.userPoolId });
    new cdk.CfnOutput(this, "UserPoolClientId", { value: userPoolClient.userPoolClientId });
    new cdk.CfnOutput(this, "NotesTableName", { value: notesTable.tableName });
  }
}
