namespace BlogProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreatedAtColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorRequests", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorRequests", "CreatedAt");
        }
    }
}
