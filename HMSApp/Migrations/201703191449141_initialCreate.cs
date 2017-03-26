namespace HMSApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Check_up",
                c => new
                    {
                        TestID = c.Int(nullable: false, identity: true),
                        PatientID = c.Int(),
                        TestName = c.String(maxLength: 50),
                        OIC = c.String(maxLength: 50),
                        Charges = c.Decimal(precision: 18, scale: 2),
                        Date = c.DateTime(),
                    })
                .PrimaryKey(t => t.TestID)
                .ForeignKey("dbo.Registration", t => t.PatientID)
                .Index(t => t.PatientID);
            
            CreateTable(
                "dbo.Registration",
                c => new
                    {
                        PatientID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Address = c.String(maxLength: 50),
                        NIC = c.String(maxLength: 12),
                        GenderID = c.Int(),
                        DOB = c.DateTime(),
                        PhoneNo = c.Int(),
                        Habits = c.String(maxLength: 50),
                        Allergic = c.String(maxLength: 50),
                        Weight = c.Decimal(precision: 18, scale: 2),
                        Height = c.Decimal(precision: 18, scale: 2),
                        Date = c.DateTime(),
                    })
                .PrimaryKey(t => t.PatientID)
                .ForeignKey("dbo.Gender", t => t.GenderID)
                .Index(t => t.GenderID);
            
            CreateTable(
                "dbo.Gender",
                c => new
                    {
                        GenderID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.GenderID);
            
            CreateTable(
                "dbo.Doctors",
                c => new
                    {
                        DoctorID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        PhoneNo = c.Int(),
                        SpecializedArea = c.String(maxLength: 50),
                        Hospital = c.String(maxLength: 50),
                        GenderID = c.Int(),
                        ConsultantDay = c.DateTime(),
                        Time = c.DateTime(),
                        ChannelingFee = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.DoctorID)
                .ForeignKey("dbo.Gender", t => t.GenderID)
                .Index(t => t.GenderID);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PayCode = c.Int(nullable: false, identity: true),
                        PatientID = c.Int(),
                        Description = c.String(maxLength: 50),
                        DoctorFee = c.Decimal(precision: 18, scale: 2),
                        PrescriptionFee = c.Decimal(precision: 18, scale: 2),
                        CheckupFee = c.Decimal(precision: 18, scale: 2),
                        HospitalFee = c.Decimal(precision: 18, scale: 2),
                        Total = c.Decimal(precision: 18, scale: 2),
                        Date = c.DateTime(),
                    })
                .PrimaryKey(t => t.PayCode)
                .ForeignKey("dbo.Registration", t => t.PatientID)
                .Index(t => t.PatientID);
            
            CreateTable(
                "dbo.Prescription",
                c => new
                    {
                        MediID = c.Int(nullable: false, identity: true),
                        DoctorID = c.Int(),
                        PatientID = c.Int(),
                        MediName = c.String(maxLength: 50),
                        Dosage = c.String(maxLength: 50),
                        TimePeriod = c.String(maxLength: 50),
                        NoOfDays = c.Int(),
                        TotalPayment = c.Decimal(precision: 18, scale: 2),
                        Date = c.DateTime(),
                    })
                .PrimaryKey(t => t.MediID)
                .ForeignKey("dbo.Doctors", t => t.DoctorID)
                .ForeignKey("dbo.Registration", t => t.PatientID)
                .Index(t => t.DoctorID)
                .Index(t => t.PatientID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Prescription", "PatientID", "dbo.Registration");
            DropForeignKey("dbo.Prescription", "DoctorID", "dbo.Doctors");
            DropForeignKey("dbo.Payments", "PatientID", "dbo.Registration");
            DropForeignKey("dbo.Doctors", "GenderID", "dbo.Gender");
            DropForeignKey("dbo.Check_up", "PatientID", "dbo.Registration");
            DropForeignKey("dbo.Registration", "GenderID", "dbo.Gender");
            DropIndex("dbo.Prescription", new[] { "PatientID" });
            DropIndex("dbo.Prescription", new[] { "DoctorID" });
            DropIndex("dbo.Payments", new[] { "PatientID" });
            DropIndex("dbo.Doctors", new[] { "GenderID" });
            DropIndex("dbo.Registration", new[] { "GenderID" });
            DropIndex("dbo.Check_up", new[] { "PatientID" });
            DropTable("dbo.Prescription");
            DropTable("dbo.Payments");
            DropTable("dbo.Doctors");
            DropTable("dbo.Gender");
            DropTable("dbo.Registration");
            DropTable("dbo.Check_up");
        }
    }
}
