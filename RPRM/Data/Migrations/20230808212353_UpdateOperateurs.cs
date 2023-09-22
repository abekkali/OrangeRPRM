using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace RPRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOperateurs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<string>(
                name: "Nom_pays",
                table: "Pays",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Nom_pays_anglais",
                table: "Pays",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetUserRoles",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetRoles",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetRoleClaims",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(450)");

            migrationBuilder.CreateTable(
                name: "asptnetUserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Permissions = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asptnetUserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asptnetUserPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operateurs",
                columns: table => new
                {
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Code_pays = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    Nom_Op = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    MCC = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    MNC = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    Marketshare = table.Column<int>(type: "int", nullable: true),
                    Op_prefered = table.Column<string>(type: "enum('oui', 'non')", nullable: true),
                    RNA = table.Column<string>(type: "enum('oui', 'non')", nullable: true),
                    RA_Teminated = table.Column<string>(type: "enum('oui', 'non')", nullable: true),
                    Type_operateur_id = table.Column<int>(type: "int", nullable: false),
                    type_accord_id = table.Column<int>(type: "int", nullable: false),
                    Code_Groupe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operateurs", x => x.Code_PLMN);
                    table.ForeignKey(
                        name: "FK_operateurs_Groupe_Code_Groupe",
                        column: x => x.Code_Groupe,
                        principalTable: "Groupe",
                        principalColumn: "Code_Groupe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_operateurs_Pays_Code_pays",
                        column: x => x.Code_pays,
                        principalTable: "Pays",
                        principalColumn: "Code_pays",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_operateurs_lookUp_table_Type_operateur_id",
                        column: x => x.Type_operateur_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_operateurs_lookUp_table_type_accord_id",
                        column: x => x.type_accord_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "test_unit",
                columns: table => new
                {
                    Code_test = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Direction_id = table.Column<int>(type: "int", nullable: true),
                    Direction_id1 = table.Column<int>(type: "int", nullable: false),
                    Nom_Service_id = table.Column<int>(type: "int", nullable: true),
                    Nom_Service_id1 = table.Column<int>(type: "int", nullable: false),
                    Etat_Test_id = table.Column<int>(type: "int", nullable: true),
                    Etat_Test_id1 = table.Column<int>(type: "int", nullable: false),
                    Commentaire_long = table.Column<string>(type: "longtext", nullable: false),
                    New_Dest = table.Column<string>(type: "longtext", nullable: false),
                    Afrique = table.Column<string>(type: "longtext", nullable: false),
                    Privilégié = table.Column<string>(type: "longtext", nullable: false),
                    Engagement = table.Column<string>(type: "longtext", nullable: false),
                    Groupe_Privilégié = table.Column<string>(type: "longtext", nullable: false),
                    Test_Owner_id = table.Column<int>(type: "int", nullable: true),
                    Test_Owner_id1 = table.Column<int>(type: "int", nullable: true),
                    date_d = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_fin = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_unit", x => x.Code_test);
                    table.ForeignKey(
                        name: "FK_test_unit_lookUp_table_Direction_id1",
                        column: x => x.Direction_id1,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_unit_lookUp_table_Etat_Test_id1",
                        column: x => x.Etat_Test_id1,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_unit_lookUp_table_Nom_Service_id1",
                        column: x => x.Nom_Service_id1,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_unit_lookUp_table_Test_Owner_id1",
                        column: x => x.Test_Owner_id1,
                        principalTable: "lookUp_table",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    Code_Contact = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(110)", maxLength: 110, nullable: true),
                    Role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact", x => x.Code_Contact);
                    table.ForeignKey(
                        name: "FK_contact_lookUp_table_Role_id",
                        column: x => x.Role_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contact_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "doc_operateur",
                columns: table => new
                {
                    Code_DOC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Document = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Type_Doc_id = table.Column<int>(type: "int", nullable: true),
                    date_d = table.Column<DateTime>(type: "date", nullable: true),
                    date_f = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doc_operateur", x => x.Code_DOC);
                    table.ForeignKey(
                        name: "FK_doc_operateur_lookUp_table_Type_Doc_id",
                        column: x => x.Type_Doc_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_doc_operateur_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "incident",
                columns: table => new
                {
                    Code_Incident = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    IMSI = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    MSISDN = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    date_d = table.Column<DateTime>(type: "date", nullable: true),
                    date_f = table.Column<DateTime>(type: "date", nullable: true),
                    Commentaire = table.Column<string>(type: "longtext", nullable: true),
                    Code_TT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Type_Incident_id = table.Column<int>(type: "int", nullable: true),
                    Direction_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident", x => x.Code_Incident);
                    table.ForeignKey(
                        name: "FK_incident_lookUp_table_Direction_id",
                        column: x => x.Direction_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_incident_lookUp_table_Type_Incident_id",
                        column: x => x.Type_Incident_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_incident_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "network_info",
                columns: table => new
                {
                    Code_Info = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", nullable: false),
                    Type_Info_id = table.Column<int>(type: "int", nullable: false),
                    Valeur = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_network_info", x => x.Code_Info);
                    table.ForeignKey(
                        name: "FK_network_info_lookUp_table_Type_Info_id",
                        column: x => x.Type_Info_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_network_info_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "services_ouverts",
                columns: table => new
                {
                    Code_Service = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Destination = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Nom_Service_id = table.Column<int>(type: "int", nullable: false),
                    date_d = table.Column<DateTime>(type: "date", nullable: true),
                    date_f = table.Column<DateTime>(type: "date", nullable: true),
                    Direction_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services_ouverts", x => x.Code_Service);
                    table.ForeignKey(
                        name: "FK_services_ouverts_lookUp_table_Direction_id",
                        column: x => x.Direction_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_services_ouverts_lookUp_table_Nom_Service_id",
                        column: x => x.Nom_Service_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_services_ouverts_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sim_received",
                columns: table => new
                {
                    Code_SIM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Code_PLMN1 = table.Column<string>(type: "varchar(5)", nullable: true),
                    IMSI = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    MSISDN = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    date_d = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_f = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sim_received", x => x.Code_SIM);
                    table.ForeignKey(
                        name: "FK_sim_received_operateurs_Code_PLMN1",
                        column: x => x.Code_PLMN1,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sim_sent",
                columns: table => new
                {
                    Code_SIM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Code_PLMN1 = table.Column<string>(type: "varchar(5)", nullable: true),
                    IMSI = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    MSISDN = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    date_d = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_f = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sim_sent", x => x.Code_SIM);
                    table.ForeignKey(
                        name: "FK_sim_sent_operateurs_Code_PLMN1",
                        column: x => x.Code_PLMN1,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tarifs",
                columns: table => new
                {
                    Code_Tarif = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code_PLMN = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    Type_Trafic_id = table.Column<int>(type: "int", nullable: false),
                    Type_Tarif_id = table.Column<int>(type: "int", nullable: false),
                    Date_d = table.Column<DateTime>(type: "date", nullable: true),
                    Date_f = table.Column<DateTime>(type: "date", nullable: true),
                    Increment_id = table.Column<int>(type: "int", nullable: false),
                    Exchange_rate = table.Column<double>(type: "double", nullable: true),
                    Rate = table.Column<double>(type: "double", nullable: true),
                    Commentaire = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Direction_id = table.Column<int>(type: "int", nullable: false),
                    Auto_Renwal = table.Column<string>(type: "enum", nullable: true),
                    Devis = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Document_DCH = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Contact = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifs", x => x.Code_Tarif);
                    table.ForeignKey(
                        name: "FK_tarifs_lookUp_table_Direction_id",
                        column: x => x.Direction_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tarifs_lookUp_table_Increment_id",
                        column: x => x.Increment_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tarifs_lookUp_table_Type_Tarif_id",
                        column: x => x.Type_Tarif_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tarifs_lookUp_table_Type_Trafic_id",
                        column: x => x.Type_Trafic_id,
                        principalTable: "lookUp_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tarifs_operateurs_Code_PLMN",
                        column: x => x.Code_PLMN,
                        principalTable: "operateurs",
                        principalColumn: "Code_PLMN",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asptnetUserPermissions_UserId",
                table: "asptnetUserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_Code_PLMN",
                table: "contact",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_contact_Role_id",
                table: "contact",
                column: "Role_id");

            migrationBuilder.CreateIndex(
                name: "IX_doc_operateur_Code_PLMN",
                table: "doc_operateur",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_doc_operateur_Type_Doc_id",
                table: "doc_operateur",
                column: "Type_Doc_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_Code_PLMN",
                table: "incident",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_incident_Direction_id",
                table: "incident",
                column: "Direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_Type_Incident_id",
                table: "incident",
                column: "Type_Incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_network_info_Code_PLMN",
                table: "network_info",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_network_info_Type_Info_id",
                table: "network_info",
                column: "Type_Info_id");

            migrationBuilder.CreateIndex(
                name: "IX_operateurs_Code_Groupe",
                table: "operateurs",
                column: "Code_Groupe");

            migrationBuilder.CreateIndex(
                name: "IX_operateurs_Code_pays",
                table: "operateurs",
                column: "Code_pays");

            migrationBuilder.CreateIndex(
                name: "IX_operateurs_type_accord_id",
                table: "operateurs",
                column: "type_accord_id");

            migrationBuilder.CreateIndex(
                name: "IX_operateurs_Type_operateur_id",
                table: "operateurs",
                column: "Type_operateur_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_ouverts_Code_PLMN",
                table: "services_ouverts",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_services_ouverts_Direction_id",
                table: "services_ouverts",
                column: "Direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_ouverts_Nom_Service_id",
                table: "services_ouverts",
                column: "Nom_Service_id");

            migrationBuilder.CreateIndex(
                name: "IX_sim_received_Code_PLMN1",
                table: "sim_received",
                column: "Code_PLMN1");

            migrationBuilder.CreateIndex(
                name: "IX_sim_sent_Code_PLMN1",
                table: "sim_sent",
                column: "Code_PLMN1");

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_Code_PLMN",
                table: "tarifs",
                column: "Code_PLMN");

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_Direction_id",
                table: "tarifs",
                column: "Direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_Increment_id",
                table: "tarifs",
                column: "Increment_id");

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_Type_Tarif_id",
                table: "tarifs",
                column: "Type_Tarif_id");

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_Type_Trafic_id",
                table: "tarifs",
                column: "Type_Trafic_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_unit_Direction_id1",
                table: "test_unit",
                column: "Direction_id1");

            migrationBuilder.CreateIndex(
                name: "IX_test_unit_Etat_Test_id1",
                table: "test_unit",
                column: "Etat_Test_id1");

            migrationBuilder.CreateIndex(
                name: "IX_test_unit_Nom_Service_id1",
                table: "test_unit",
                column: "Nom_Service_id1");

            migrationBuilder.CreateIndex(
                name: "IX_test_unit_Test_Owner_id1",
                table: "test_unit",
                column: "Test_Owner_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asptnetUserPermissions");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "doc_operateur");

            migrationBuilder.DropTable(
                name: "incident");

            migrationBuilder.DropTable(
                name: "network_info");

            migrationBuilder.DropTable(
                name: "services_ouverts");

            migrationBuilder.DropTable(
                name: "sim_received");

            migrationBuilder.DropTable(
                name: "sim_sent");

            migrationBuilder.DropTable(
                name: "tarifs");

            migrationBuilder.DropTable(
                name: "test_unit");

            migrationBuilder.DropTable(
                name: "operateurs");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Nom_pays_anglais",
                table: "Pays");

            migrationBuilder.AlterColumn<string>(
                name: "Nom_pays",
                table: "Pays",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetUserRoles",
                type: "varchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetRoles",
                type: "varchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetRoleClaims",
                type: "varchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }
    }
}
