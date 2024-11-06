using Pinecone;

var pinecone = new PineconeClient("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

var index = await pinecone.CreateIndexAsync(new CreateIndexRequest
{
    Name = "example-index",
    Dimension = 1538,
    Metric = CreateIndexRequestMetric.Cosine,
    Spec = new ServerlessIndexSpec
    {
        Serverless = new ServerlessSpec
        {
            Cloud = ServerlessSpecCloud.Azure,
            Region = "eastus2",
        }
    },
    DeletionProtection = DeletionProtection.Enabled
});

//Once you created the index, then use the following code: 
//var index = pinecone.Index("example-index");

if (index == null)
{
    Console.WriteLine("Index not found or initialization failed.");
    return;
}

// Define vector IDs to be upserted
var upsertIds = new[] { "v1", "v2", "v3" };

// Dummy vector values with 1538 dimensions
float[] dummyValues = new float[1538];
for (int j = 0; j < dummyValues.Length; j++) dummyValues[j] = 1.0f; // Fill with some dummy data

float[][] values =
{
    dummyValues,
    dummyValues,
    dummyValues,
};

// Metadata to be upserted
var metadataStructArray = new[]
{
    new Metadata { ["genre"] = "action", ["year"] = 2019 },
    new Metadata { ["genre"] = "thriller", ["year"] = 2020 },
    new Metadata { ["genre"] = "comedy", ["year"] = 2021 },
};

var vectors = new List<Vector>();
for (var i = 0; i < 3; i++)
{
    vectors.Add(
        new Vector
        {
            Id = upsertIds[i],
            Values = values[i],
            Metadata = metadataStructArray[i],
        }
    );
    Console.WriteLine($"Prepared vector {upsertIds[i]} with dimension: {values[i].Length}");
}

// Upsert the vectors
try
{
    var upsertResponse = await index.UpsertAsync(new UpsertRequest { Vectors = vectors });
    Console.WriteLine("Upsert completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during upsert: {ex.Message}");
}

// Prepare a query vector with 1538 dimensions
float[] queryVector = new float[1538];
for (int j = 0; j < queryVector.Length; j++) queryVector[j] = 0.1f; // Fill with some sample data

// Perform a query to find similar vectors
try
{
    var queryResponse = await index.QueryAsync(
       new QueryRequest
       {
           Namespace = null, // Set to null if no namespace was used during upsert
           Vector = queryVector, // Use the 1538-dimension query vector
           TopK = 10,
           IncludeValues = true,
           IncludeMetadata = true,
           Filter = new Metadata
           {
               ["genre"] = new Metadata
               {
                   ["$in"] = new[] { "comedy", "action", "thriller" }, // Simplified filter for existing genres
               }
           }
       });

    // Display query results
    if (queryResponse.Matches == null)
    {
        Console.WriteLine("No matches found for the query.");
    }
    else
    {
        Console.WriteLine("Query Results:");
        foreach (var match in queryResponse.Matches)
        {
            Console.WriteLine($"ID: {match.Id}, Score: {match.Score}");
            if (match.Values != null)
            {
                Console.WriteLine("Values: " + string.Join(", ", match.Values));
            }
            if (match.Metadata != null)
            {
                Console.WriteLine("Metadata: ");
                foreach (var key in match.Metadata.Keys)
                {
                    Console.WriteLine($"{key}: {match.Metadata[key]}");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during query: {ex.Message}");
}


