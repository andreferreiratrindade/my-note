using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using System.Collections.Generic;

namespace Infra2
{
    public class Infra2Stack : Stack
    {
        internal Infra2Stack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // 1. DynamoDB Table
            var table = new Table(this, "MyTable", new TableProps
            {
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // 2. Cognito User Pool
            var userPool = new UserPool(this, "MyUserPool", new UserPoolProps
            {
                UserPoolName = "MyUserPool",
                SelfSignUpEnabled = true,
                StandardAttributes = new StandardAttributes
                {
                    Email = new StandardAttribute { Required = true, Mutable = true }
                },
                SignInAliases = new SignInAliases { Email = true }
            });

            var userPoolClient = new UserPoolClient(this, "MyUserPoolClient", new UserPoolClientProps
            {
                UserPool = userPool,
                GenerateSecret = false
            });

            // 3. C# Lambda Function
            var myLambda = new Function(this, "MyLambdaHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("../lambda/MyNote/MyNote/src/MyNote/bin/Release/net8.0/publish"),
                Handler = "MyNote::MyNote.Function::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "TABLE_NAME", table.TableName }
                }
            });
            table.GrantReadWriteData(myLambda);

            // 4. API Gateway with Cognito Authorizer
            var authorizer = new CognitoUserPoolsAuthorizer(this, "MyAuthorizer", new CognitoUserPoolsAuthorizerProps
            {
                CognitoUserPools = new[] { userPool }
            });

            var api = new RestApi(this, "MyRestAPI", new RestApiProps
            {
                RestApiName = "MyService"
            });

            var items = api.Root.AddResource("items");
            var item = items.AddResource("{id}");

            items.AddMethod("POST", new LambdaIntegration(myLambda), new MethodOptions
            {
                Authorizer = authorizer,
                AuthorizationType = AuthorizationType.COGNITO
            });

            item.AddMethod("GET", new LambdaIntegration(myLambda), new MethodOptions
            {
                Authorizer = authorizer,
                AuthorizationType = AuthorizationType.COGNITO
            });
        }
    }
}
