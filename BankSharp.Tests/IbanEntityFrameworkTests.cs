using System;
using System.Threading.Tasks;
using BankSharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Contains integration tests verifying Entity Framework Core support for the <see cref="Iban"/> type,
/// including <see cref="IbanValueConverter"/> and <see cref="ModelBuilderExtensions.UseIbanConversions"/>.
/// </summary>
public class IbanEntityFrameworkTests
{
    /// <summary>
    /// Test entity representing a customer with both required and nullable IBAN properties.
    /// </summary>
    private class TestCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Iban AccountIban { get; set; }
        public Iban? OptionalIban { get; set; }
    }

    /// <summary>
    /// In-memory test DbContext configured with BankSharp IBAN value conversions.
    /// </summary>
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestCustomer> Customers => Set<TestCustomer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically discover and register IbanValueConverter for all Iban properties
            modelBuilder.UseIbanConversions();
        }
    }

    /// <summary>
    /// Verifies that <see cref="IbanValueConverter"/> correctly maps an <see cref="Iban"/> instance
    /// to its underlying database string representation and reconstructs the object identically.
    /// </summary>
    [Fact]
    public void IbanValueConverter_ShouldDirectlyConvertToStringAndBack()
    {
        // Arrange
        var converter = new IbanValueConverter();
        var originalIban = Iban.Parse("DE89370400440532013000");

        // Act: 1. Convert Iban to database string representation
        var dbValue = (string)converter.ConvertToProvider(originalIban)!;

        // Assert: Ensure converted string is plain and unformatted
        Assert.Equal("DE89370400440532013000", dbValue);

        // Act: 2. Convert provider string back to strongly-typed Iban object
        var restoredIban = (Iban)converter.ConvertFromProvider(dbValue)!;

        // Assert: Both instances must be structurally and value equivalent
        Assert.Equal(originalIban, restoredIban);
    }

    /// <summary>
    /// Verifies that EF Core successfully persists required and optional IBAN fields
    /// to the database and retrieves them intact via queries.
    /// </summary>
    [Fact]
    public async Task DbContext_ShouldSaveAndRetrieveIbanCorrectly()
    {
        // Arrange: Setup unique in-memory database instance
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var ibanValue = Iban.Parse("IR820540102680020817909002");

        // Act: 1. Seed entity data into the database
        await using (var context = new TestDbContext(options))
        {
            var customer = new TestCustomer
            {
                Id = 1,
                Name = "Ehsan",
                AccountIban = ibanValue,
                OptionalIban = null
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Act & Assert: 2. Query data in a fresh DbContext and verify persistence integrity
        await using (var context = new TestDbContext(options))
        {
            var savedCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Id == 1);

            Assert.NotNull(savedCustomer);
            Assert.Equal("Ehsan", savedCustomer.Name);
            Assert.Equal(ibanValue, savedCustomer.AccountIban);
            Assert.Null(savedCustomer.OptionalIban);

            // Verify LINQ predicate filtering by Iban value works seamlessly
            var filteredCustomer = await context.Customers.FirstOrDefaultAsync(c => c.AccountIban == ibanValue);
            Assert.NotNull(filteredCustomer);
            Assert.Equal(1, filteredCustomer.Id);
        }
    }

    /// <summary>
    /// Verifies that nullable IBAN properties correctly persist and restore non-null values.
    /// </summary>
    [Fact]
    public async Task DbContext_ShouldHandlePopulatedOptionalIban()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var primaryIban = Iban.Parse("GB29NWBK60161331926819");
        var secondaryIban = Iban.Parse("FR1420041010050500013M02606");

        // Act: Persist customer with both primary and optional IBAN populated
        await using (var context = new TestDbContext(options))
        {
            var customer = new TestCustomer
            {
                Id = 2,
                Name = "Developer",
                AccountIban = primaryIban,
                OptionalIban = secondaryIban
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        // Assert: Verify both IBAN instances are preserved accurately
        await using (var context = new TestDbContext(options))
        {
            var retrieved = await context.Customers.FindAsync(2);

            Assert.NotNull(retrieved);
            Assert.Equal(primaryIban, retrieved.AccountIban);
            Assert.Equal(secondaryIban, retrieved.OptionalIban);
        }
    }
}
