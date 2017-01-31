namespace WU15.Azure.ServiceManager.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_ServiceTicket_Model : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceTicket",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Description = c.String(),
                        Done = c.Boolean(nullable: false),
                        TicketIsWithdrawn = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        DoneDate = c.DateTime(),
                        CustomerTicketId = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        CustomerEmail = c.String(),
                        ResponsibleUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ResponsibleUser_Id)
                .Index(t => t.ResponsibleUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceTicket", "ResponsibleUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ServiceTicket", new[] { "ResponsibleUser_Id" });
            DropTable("dbo.ServiceTicket");
        }
    }
}
