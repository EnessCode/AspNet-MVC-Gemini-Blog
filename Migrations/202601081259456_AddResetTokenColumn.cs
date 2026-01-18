namespace BlogProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResetTokenColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ResetToken", c => c.String());
            AddColumn("dbo.Users", "ResetTokenExpires", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ResetTokenExpires");
            DropColumn("dbo.Users", "ResetToken");
        }
    }
}
