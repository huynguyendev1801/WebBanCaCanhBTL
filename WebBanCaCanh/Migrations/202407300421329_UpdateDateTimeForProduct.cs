namespace WebBanCaCanh.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDateTimeForProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Products", "UpdatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "UpdatedAt");
            DropColumn("dbo.Products", "CreatedAt");
        }
    }
}
