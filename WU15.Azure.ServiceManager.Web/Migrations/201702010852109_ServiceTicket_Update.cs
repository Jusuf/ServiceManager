namespace WU15.Azure.ServiceManager.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceTicket_Update : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceTicketViewModel", "ResponsibleUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ServiceTicketViewModel", "ResponsibleUser_Id");
            AddForeignKey("dbo.ServiceTicketViewModel", "ResponsibleUser_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceTicketViewModel", "ResponsibleUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ServiceTicketViewModel", new[] { "ResponsibleUser_Id" });
            DropColumn("dbo.ServiceTicketViewModel", "ResponsibleUser_Id");
        }
    }
}
