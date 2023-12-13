using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CopyOldCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DECLARE @ItemId INT
                                    DECLARE @ItemValue NVARCHAR(MAX)
                                    DECLARE @LocalizationSetId INT

                                    DECLARE ItemsCursor CURSOR FOR

                                    SELECT Id FROM Categories

                                    OPEN ItemsCursor
                                    FETCH NEXT FROM ItemsCursor INTO @ItemId

                                    WHILE @@FETCH_STATUS=0

                                    BEGIN
                                        SELECT @ItemValue = [Name] FROM Categories WHERE Id = @ItemId

                                        INSERT INTO LocalizationSets VALUES('Category_Name')

                                        SELECT @LocalizationSetId = SCOPE_IDENTITY()

                                        INSERT INTO Localizations VALUES(@LocalizationSetId, 'en-US', @ItemValue)
                                        INSERT INTO Localizations VALUES(@LocalizationSetId, 'ar-EG', @ItemValue + N' - عربي')

                                        UPDATE Categories SET NameId = @LocalizationSetId WHERE Id = @ItemId

                                        FETCH NEXT FROM ItemsCursor INTO @ItemId
                                    END

                                    CLOSE ItemsCursor
                                    DEALLOCATE ItemsCursor"
                                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Categories SET NameId = NULL
                                    DELETE FROM Localizations
                                    DELETE FROM LocalizationSets

                                    DBCC CHECKIDENT ('LocalizationSets', RESEED, 0)"
                                );
        }
    }
}
