namespace BlogProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserLockout : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "FailedLoginAttempts", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "LockoutEnd", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "LockoutEnd");
            DropColumn("dbo.Users", "FailedLoginAttempts");
        }
    }
}
