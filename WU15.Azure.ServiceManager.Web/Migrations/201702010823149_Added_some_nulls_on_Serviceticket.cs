namespace WU15.Azure.ServiceManager.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_some_nulls_on_Serviceticket : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceTicketViewModel",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Description = c.String(nullable: false, maxLength: 200),
                        Done = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        DoneDate = c.String(),
                        CustomerEmail = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.ServiceTicket", "CustomerTicketId", c => c.Guid());
            AlterColumn("dbo.ServiceTicket", "CustomerId", c => c.Guid());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ServiceTicket", "CustomerId", c => c.Guid(nullable: false));
            AlterColumn("dbo.ServiceTicket", "CustomerTicketId", c => c.Guid(nullable: false));
            DropTable("dbo.ServiceTicketViewModel");
        }
    }
}
