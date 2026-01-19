namespace BlogProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdminPickColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "IsEditorPick", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "IsEditorPick");
        }
    }
}
