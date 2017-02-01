namespace WU15.Azure.ServiceManager.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migration_fix_removed_ServiceTicketViewModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ServiceTicketViewModel", "ResponsibleUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ServiceTicketViewModel", new[] { "ResponsibleUser_Id" });
            DropTable("dbo.ServiceTicketViewModel");
        }
        
        public override void Down()
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
                        ResponsibleUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ServiceTicketViewModel", "ResponsibleUser_Id");
            AddForeignKey("dbo.ServiceTicketViewModel", "ResponsibleUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
