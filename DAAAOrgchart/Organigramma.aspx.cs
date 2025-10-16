using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Linq;
using System.Configuration;

namespace DAAAOrgchart
{
    public partial class Organigramma : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GenerateOrganigramma();
            }
        }

        private void GenerateOrganigramma()
        {
            var departments = LoadDepartments();
            var staff = LoadStaff();

            var hierarchy = new Dictionary<string, List<Department>>();

            // 🔹 Costruzione gerarchia
            foreach (var dept in departments)
            {
                string parentKey = string.IsNullOrEmpty(dept.ParentId) ? "root" : dept.ParentId;
                if (!hierarchy.ContainsKey(parentKey))
                    hierarchy[parentKey] = new List<Department>();

                hierarchy[parentKey].Add(dept);
            }

            var sb = new StringBuilder();
            sb.Append("<ul class='tree'>");

            // 🔹 Nodo root (Direttore = ID 11)
            if (hierarchy.ContainsKey("11"))
            {
                foreach (var rootDept in hierarchy["11"])
                    BuildTreeHTML(rootDept, hierarchy, staff, sb);
            }

            sb.Append("</ul>");
            OrganigrammaLiteral.Text = sb.ToString();
        }

        private void BuildTreeHTML(Department dept, Dictionary<string, List<Department>> hierarchy,
                                   List<StaffMember> staff, StringBuilder sb)
        {
            sb.Append("<li>");
            sb.AppendFormat("<strong>{0}</strong>", dept.Name);

            // 🔹 Mostra il responsabile se presente
            if (!string.IsNullOrEmpty(dept.ManagerFullName))
            {
                sb.AppendFormat("<div class='person'><b>Responsabile:</b> {0}</div>", dept.ManagerFullName);
            }

            // 🔹 Personale collegato a questo dipartimento
            var assigned = staff.Where(s => s.DepartmentId == dept.Id).ToList();
            if (assigned.Any())
            {
                sb.Append("<ul>");
                foreach (var s in assigned)
                {
                    sb.AppendFormat("<li>{0} – <em>{1}</em></li>", s.FullName, s.Role);
                }
                sb.Append("</ul>");
            }

            // 🔹 Sotto-nodi
            if (hierarchy.ContainsKey(dept.Id))
            {
                sb.Append("<ul>");
                foreach (var child in hierarchy[dept.Id])
                    BuildTreeHTML(child, hierarchy, staff, sb);
                sb.Append("</ul>");
            }

            sb.Append("</li>");
        }

        // 🔹 Carica i dipartimenti con il responsabile
        private List<Department> LoadDepartments()
        {
            var list = new List<Department>();
            string connStr = ConfigurationManager.ConnectionStrings["DipendentiDBConnectionString"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 🔸 vOrganigrammaCompleto deve avere: id, parent_id, name, manager_id
                string query = @"
                    SELECT 
                        v.id, 
                        v.parent_id, 
                        v.name,
                        v.manager_id,
                        p.Nome AS ManagerNome,
                        p.Cognome AS ManagerCognome
                    FROM vOrganigrammaCompleto v
                    LEFT JOIN ElencoPersonale p ON p.IDPersonale = v.manager_id
                    WHERE p.Stato_Servizio = 'Attivo' OR v.manager_id IS NULL
                    ORDER BY v.id;";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nome = reader["ManagerNome"] == DBNull.Value ? "" : reader["ManagerNome"].ToString();
                        string cognome = reader["ManagerCognome"] == DBNull.Value ? "" : reader["ManagerCognome"].ToString();

                        list.Add(new Department
                        {
                            Id = reader["id"].ToString(),
                            ParentId = reader["parent_id"] == DBNull.Value ? null : reader["parent_id"].ToString(),
                            Name = reader["name"].ToString(),
                            ManagerId = reader["manager_id"] == DBNull.Value ? null : reader["manager_id"].ToString(),
                            ManagerFullName = (!string.IsNullOrEmpty(nome) || !string.IsNullOrEmpty(cognome))
                                ? $"{nome} {cognome}"
                                : null
                        });
                    }
                }
            }

            return list;
        }

        // 🔹 Carica il personale (già ok)
        private List<StaffMember> LoadStaff()
        {
            var list = new List<StaffMember>();
            string connStr = ConfigurationManager.ConnectionStrings["DipendentiDBConnectionString"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        CONCAT(
                            CAST(ISNULL(i.ID_Uff1, '') AS NVARCHAR(50)),
                            CASE WHEN i.ID_Uff2 IS NOT NULL THEN '_' + CAST(i.ID_Uff2 AS NVARCHAR(50)) ELSE '' END,
                            CASE WHEN i.ID_Uff3 IS NOT NULL THEN '_' + CAST(i.ID_Uff3 AS NVARCHAR(50)) ELSE '' END
                        ) AS department_id,
                        i.IDPersonale AS person_id,
                        p.Cognome + ' ' + p.Nome AS full_name,
                        i.id_incarico AS role
                    FROM Incarichi i
                    INNER JOIN ElencoPersonale p ON p.IDPersonale = i.IDPersonale
                    WHERE p.Stato_Servizio = 'Attivo';";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new StaffMember
                        {
                            DepartmentId = reader["department_id"].ToString(),
                            PersonId = Convert.ToInt32(reader["person_id"]),
                            FullName = reader["full_name"].ToString(),
                            Role = reader["role"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        // 🔹 Classi di supporto
        public class Department
        {
            public string Id { get; set; }
            public string ParentId { get; set; }
            public string Name { get; set; }
            public string ManagerId { get; set; }
            public string ManagerFullName { get; set; }
            public List<Department> Children { get; set; } = new List<Department>();
        }

        public class StaffMember
        {
            public string DepartmentId { get; set; }
            public int PersonId { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
        }
    }
}
