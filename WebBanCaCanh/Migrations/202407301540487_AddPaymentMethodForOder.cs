namespace WebBanCaCanh.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentMethodForOder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "PaymentMethod", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "PaymentMethod");
        }
    }
}
