using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MyNote.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyNote;

public class NoteService
{
        private readonly DynamoDBContext _dbContext;
        public readonly Dictionary<string, string> Headers = new Dictionary<string, string> { { "Access-Control-Allow-Origin", "*" } };
        private readonly JsonSerializerSettings _options = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        public NoteService(IAmazonDynamoDB dynamoDbClient)
        {
            _dbContext = new DynamoDBContext(dynamoDbClient);
        }

        public async Task<APIGatewayProxyResponse> CreateNoteAsync(string userId, Note newNote)
        {
            if (string.IsNullOrEmpty(newNote.Title))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Note title cannot be empty.",
                    Headers =Headers
                };
            }

            newNote.UserId = userId;
            newNote.NoteId = Guid.NewGuid().ToString();
            newNote.UpdatedAt = DateTime.UtcNow.ToLongDateString();
            newNote.CreatedAt = DateTime.UtcNow.ToLongDateString();

            await _dbContext.SaveAsync(newNote);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonConvert.SerializeObject(newNote, _options),
                Headers =Headers
            };
        }

        public async Task<APIGatewayProxyResponse> GetNotesAsync(string userId)
        {
            var notes = await _dbContext.QueryAsync<Note>(userId).GetRemainingAsync();
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(notes, _options),
                Headers =Headers
            };
        }

        public async Task<APIGatewayProxyResponse> GetNoteByIdAsync(string userId, string noteId)
        {
            var note = await _dbContext.LoadAsync<Note>(userId, noteId);
            if (note == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = "Note not found.",
                    Headers = Headers
                };
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(note, _options),
                Headers = Headers
            };
        }

        public async Task<APIGatewayProxyResponse> UpdateNoteAsync(string userId, string noteId, Note updatedNote)
    {
        var noteToUpdate = await _dbContext.LoadAsync<Note>(userId, noteId);
        if (noteToUpdate == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "Note not found.",
                Headers = Headers
            };
        }

        noteToUpdate.Title = updatedNote.Title ?? noteToUpdate.Title;
        noteToUpdate.Content = updatedNote.Content;
        noteToUpdate.UpdatedAt = DateTime.UtcNow.ToLongDateString();

        await _dbContext.SaveAsync(noteToUpdate);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonConvert.SerializeObject(noteToUpdate, _options),
            Headers = Headers
        };
    }

        public async Task<APIGatewayProxyResponse> DeleteNoteAsync(string userId, string noteId)
        {
            await _dbContext.DeleteAsync<Note>(userId, noteId);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent,
                Headers =Headers
            };
        }
    }
