using Amazon.DynamoDBv2.DataModel;

namespace MyNote.Models;


[DynamoDBTable("notes")]
public class Note
{
    [DynamoDBHashKey("userId")]

    public string UserId { get; set; }

    [DynamoDBRangeKey("noteId")]
    public string NoteId { get; set; }

    [DynamoDBProperty("content")]
    public string Content { get; set; }

    [DynamoDBProperty("title")]
    public string Title { get; set; }

    [DynamoDBProperty("createdAt")]
    public string CreatedAt { get; set; }

    [DynamoDBProperty("updatedAt")]
    public string UpdatedAt { get; set; }
}
