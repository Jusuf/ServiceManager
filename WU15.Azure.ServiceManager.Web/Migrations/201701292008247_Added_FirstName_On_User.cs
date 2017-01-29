namespace WU15.Azure.ServiceManager.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_FirstName_On_User : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FirstName");
        }
    }
}
