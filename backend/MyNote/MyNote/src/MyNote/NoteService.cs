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

namespace MyNote;

public class NoteService
{
        private readonly DynamoDBContext _dbContext;

        public NoteService(IAmazonDynamoDB dynamoDbClient)
        {
            _dbContext = new DynamoDBContext(dynamoDbClient);
        }

        public async Task<APIGatewayProxyResponse> CreateNoteAsync(string userId, Note newNote)
        {
            if (string.IsNullOrEmpty(newNote.Content))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Note content cannot be empty."
                };
            }

            newNote.UserId = userId;
            newNote.NoteId = Guid.NewGuid().ToString();
            newNote.UpdatedAt = DateTime.UtcNow.ToLongDateString();

            await _dbContext.SaveAsync(newNote);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonConvert.SerializeObject(newNote)
            };
        }

        public async Task<APIGatewayProxyResponse> GetNotesAsync(string userId)
        {
            var notes = await _dbContext.QueryAsync<Note>(userId).GetRemainingAsync();
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(notes)
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
                    Body = "Note not found."
                };
            }

            noteToUpdate.Content = updatedNote.Content ?? noteToUpdate.Content;
            noteToUpdate.UpdatedAt = DateTime.UtcNow.ToLongDateString();

            await _dbContext.SaveAsync(noteToUpdate);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(noteToUpdate)
            };
        }

        public async Task<APIGatewayProxyResponse> DeleteNoteAsync(string userId, string noteId)
        {
            await _dbContext.DeleteAsync<Note>(userId, noteId);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NoContent
            };
        }
    }
