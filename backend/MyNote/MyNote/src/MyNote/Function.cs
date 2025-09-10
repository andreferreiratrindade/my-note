
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MyNote.Models;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]


namespace MyNote;

public class Function
{
    private readonly NoteService _noteService;


    // Constructor to inject the DynamoDB service.
    public Function()
    {
        var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
        _noteService = new NoteService(client);
    }

    /// <summary>
    /// This method is the main entry point for the Lambda function.
    /// It acts as a router, directing the request to the correct method
    /// based on the HTTP method and request path.
    /// </summary>
    /// <param name="request">The API Gateway proxy request object.</param>
    /// <param name="context">The Lambda execution context.</param>
    /// <returns>An API Gateway proxy response object.</returns>
    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine($"Received {request.HttpMethod} request for path {request.Path}");
            context.Logger.LogLine($"Request: {request}");
            // A quick check to make sure the user is authenticated.
            // In a real application, you would handle this more robustly.
            var userId = request.RequestContext.Authorizer?.Claims["sub"] ; 
            var pathParameters = request.PathParameters;


            // This switch statement handles the routing logic.
            // It checks the HTTP method and the request path to determine which
            // NoteService method to call.
            switch (request.HttpMethod)
            {
                case "GET":
                    
                    
                    if (pathParameters != null && pathParameters.ContainsKey("noteId"))
                    {
                        var noteId = pathParameters["noteId"];
                        return await _noteService.GetNoteByIdAsync(userId, noteId);
                    }
                    return await _noteService.GetNotesAsync(userId);

                case "POST":

                    var newNote = JsonConvert.DeserializeObject<Note>(request.Body);
                    return await _noteService.CreateNoteAsync(userId, newNote);

                case "PUT":
                    
                    if (pathParameters != null && pathParameters.ContainsKey("noteId"))
                    {
                        var noteId = pathParameters["noteId"];
                        var updatedNote =  JsonConvert.DeserializeObject<Note>(request.Body);
                        return await _noteService.UpdateNoteAsync(userId, noteId, updatedNote);
                    }
                    return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Note ID is missing from the request path." ,
                        Headers = _noteService.Headers

                    };

                case "DELETE":
                    
                    if (pathParameters != null && pathParameters.ContainsKey("noteId"))
                    {
                        var noteId = pathParameters["noteId"];
                        return await _noteService.DeleteNoteAsync(userId, noteId);
                    }
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Note ID is missing from the request path.",
                        Headers = _noteService.Headers
                    };

                default:
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Body = "Method not found.",
                        Headers = _noteService.Headers

                    };
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"An error occurred: {ex.Message}");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"An internal server error occurred: {ex.Message}",
                Headers = _noteService.Headers
            };
        }
    }
}
