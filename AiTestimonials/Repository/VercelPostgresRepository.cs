using AiTestimonials.Models;
using AiTestimonials.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AiTestimonials.Repository;

public class VercelPostgresRepository
{

    private readonly NpgsqlDataSource _db;

    public VercelPostgresRepository(IOptions<ServiceOptions> serviceOptions)
    {
        var connectionString = serviceOptions.Value.PostgresConnectionString;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        _db = dataSourceBuilder.Build();
    }

    public async Task<bool> TableExistsAsync(string table)
    {
        await using var cmd = _db.CreateCommand($"SELECT EXISTS ( SELECT FROM pg_tables WHERE tablename = '{table}');");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return reader.GetBoolean(0);
        }

        return false;
    }

    public async Task CreatTestimonialsTableAsync()
    {
        var tableExists = await TableExistsAsync("testimonials");
        if (!tableExists)
        {

            await using var cmd = _db.CreateCommand(
                $@"
                CREATE TABLE testimonials (
                id SERIAL PRIMARY KEY,
                status int,
                input JSONB,
                testimonial JSONB
                );"
            );
            await cmd.ExecuteReaderAsync();
        }
    }

    public async Task<int> CreateNewTestimonialAsync(TestimonialInput input)
    {
        await using var cmd = _db.CreateCommand($"INSERT INTO testimonials (status, input) VALUES (@status, @input) RETURNING id;");
        var param1 = new NpgsqlParameter() { ParameterName = "status", NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer, Value = (int)TestimonialStatus.PENDING };
        var param2 = new NpgsqlParameter() { ParameterName = "input", NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb, Value = input };
        cmd.Parameters.Add(param1);
        cmd.Parameters.Add(param2);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetFieldValue<int>(0);
            return id;
        }
        throw new InvalidOperationException();
    }

    public async Task AddTestimonialAsync(TestimonialResult testimonialResult, string id)
    {
        await using var cmd = _db.CreateCommand($"UPDATE testimonials SET testimonial = @testimonial WHERE id = {id};");
        var param = new NpgsqlParameter() { ParameterName = "testimonial", NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb, Value = testimonialResult };
        cmd.Parameters.Add(param);
        await using var reader = await cmd.ExecuteReaderAsync();
    }

    public async Task UpdateTestimonialAsync(TestimonialStatus status, string id)
    {
        await using var cmd = _db.CreateCommand($"UPDATE testimonials SET status = @status WHERE id = {id};");
        var param = new NpgsqlParameter() { ParameterName = "status", NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer, Value = (int)status };
        cmd.Parameters.Add(param);
        await using var reader = await cmd.ExecuteReaderAsync();
    }

    public async Task<TestimonialEntity?> GetTestimonialsEntityAsync(string id)
    {

        var tableExists = await TableExistsAsync("testimonials");

        if (tableExists)
        {
            var query = $"SELECT status, input, testimonial  FROM testimonials WHERE id = {id}";
            await using var cmd = _db.CreateCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var status = (TestimonialStatus)reader.GetFieldValue<int>(0);
                var input = reader.GetFieldValue<TestimonialInput>(1);
                var testimonial = reader.GetFieldValue<TestimonialResult>(2);
                return new TestimonialEntity()
                {
                    Id = id,
                    Status = status,
                    Input = input,
                    Testimonial = testimonial
                };
            }
        }
        return null;
    }

    public async Task<List<TestimonialResult>> GetTestimonialsAsync(string? id)
    {

        var tableExists = await TableExistsAsync("testimonials");
        List<TestimonialResult> res = [];

        if (tableExists)
        {
            var query = id != null
                ? $"SELECT testimonial FROM testimonials WHERE id = {id} AND status = {(int)TestimonialStatus.SUCCESSFUL}"
                : $"SELECT testimonial FROM testimonials WHERE status = {(int)TestimonialStatus.SUCCESSFUL}";
            await using var cmd = _db.CreateCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var testimonial = reader.GetFieldValue<TestimonialResult>(0);
                if (testimonial != null)
                    res.Add(testimonial);
            }
        }
        return res;
    }

}

public record BookingEntity
{
    public string? Id { get; init; }
    public string? Date { get; init; }
    public bool? Confirmed { get; init; }
}