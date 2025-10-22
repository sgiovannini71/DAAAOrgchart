using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DipendentiWeb
{

        public partial class Organigramma : System.Web.UI.Page
        {
            private string connString = ConfigurationManager.ConnectionStrings["DipendentiDBConnectionString"].ConnectionString;

            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    litOrganigramma.Text = CreaOrganigramma();
                }
            }

            private string CreaOrganigramma()
            {
                StringBuilder sb = new StringBuilder();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Direttore
                    sb.Append("<h2>Direzione Generale</h2>");
                    sb.Append(GetUfficioPrincipale(con, 11, "Direttore"));

                    // Prime linee dirigenziali (Liv1Uff ma escluso ID=11 e Livello=0)
                    sb.Append("<h3>Prime Linee Dirigenziali</h3>");
                    string queryPrimeLinee = "SELECT ID_Uff1, SgUff1, DescUff1 FROM Liv1Uff WHERE ID_Uff1 <> 11 AND (Livello IS NULL OR Livello > 0) ORDER BY SgUff1";

                    using (SqlCommand cmd = new SqlCommand(queryPrimeLinee, con))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int idUff1 = Convert.ToInt32(dr["ID_Uff1"]);
                            string nome = $"{dr["SgUff1"]} - {dr["DescUff1"]}";
                            sb.Append($"<div class='ufficio'><div class='titolo'>{nome}</div>");

                            string resp = GetResponsabile(con, idUff1, null, null);
                            if (!string.IsNullOrEmpty(resp))
                                sb.Append($"<div class='responsabile'>Responsabile: {resp}</div>");
                        // ---------------- aggiungi qui gli addetti ----------------
                        string addetti = GetAddetti(con, idUff1, null, null);
                        if (!string.IsNullOrEmpty(addetti))
                            sb.Append($"<div class='addetti'>Addetti: {addetti}</div>");
                        // ---------------------------------------------------------

                        // Liv2 e Liv3 per questa direzione
                        sb.Append(GetLiv2(con, idUff1, 1));
                            sb.Append("</div>");
                        }
                    }

                    // Uffici di staff (Liv1Uff con Livello=0)
                    sb.Append("<h3>Uffici di Staff</h3>");
                    string queryStaff = "SELECT ID_Uff1, SgUff1, DescUff1 FROM Liv1Uff WHERE Livello = 0 ORDER BY SgUff1";

                    using (SqlCommand cmd = new SqlCommand(queryStaff, con))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int idUff1 = Convert.ToInt32(dr["ID_Uff1"]);
                            string nome = $"{dr["SgUff1"]} - {dr["DescUff1"]}";
                            sb.Append($"<div class='ufficio'><div class='titolo'>{nome}</div>");

                            string resp = GetResponsabile(con, idUff1, null, null);
                            if (!string.IsNullOrEmpty(resp))
                                sb.Append($"<div class='responsabile'>Responsabile: {resp}</div>");

                            sb.Append("</div>");
                        }
                    }
                }

                return sb.ToString();
            }

            private string GetUfficioPrincipale(SqlConnection con, int idUff1, string titolo)
            {
                StringBuilder sb = new StringBuilder();

                string query = "SELECT SgUff1, DescUff1 FROM Liv1Uff WHERE ID_Uff1=@id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", idUff1);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string nome = $"{dr["SgUff1"]} - {dr["DescUff1"]}";
                            sb.Append($"<div class='titolo'>{titolo}: {nome}</div>");
                        }
                    }
                }

                string resp = GetResponsabile(con, idUff1, null, null);
                if (!string.IsNullOrEmpty(resp))
                    sb.Append($"<div class='responsabile'>Responsabile: {resp}</div>");

                return sb.ToString();
            }

            private string GetLiv2(SqlConnection con, int idUff1, int livello)
            {
                StringBuilder sb = new StringBuilder();
                string query = "SELECT ID_Uff2, SgUff2, DescUff2 FROM Liv2Uff WHERE ID_Uff1=@idUff1 ORDER BY SgUff2";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@idUff1", idUff1);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int idUff2 = Convert.ToInt32(dr["ID_Uff2"]);
                            string nome = $"{dr["SgUff2"]} - {dr["DescUff2"]}";
                            sb.Append($"<div class='ufficio' style='margin-left:{livello * 25}px'>");
                            sb.Append($"<div class='titolo'>{nome}</div>");

                            string resp2 = GetResponsabile(con, idUff1, idUff2, null);
                            if (!string.IsNullOrEmpty(resp2))
                                sb.Append($"<div class='responsabile'>Responsabile: {resp2}</div>");
                        // ---------------- aggiungi qui gli addetti per Liv2 ----------------
                        string addettiLiv2 = GetAddetti(con, idUff1, idUff2, null);
                        if (!string.IsNullOrEmpty(addettiLiv2))
                            sb.Append($"<div class='addetti'>Addetti: {addettiLiv2}</div>");
                        // ------------------------------------------------------------------

                        sb.Append(GetLiv3(con, idUff1, idUff2, livello + 1));
                            sb.Append("</div>");
                        }
                    }
                }
                return sb.ToString();
            }

            private string GetLiv3(SqlConnection con, int idUff1, int idUff2, int livello)
            {
                StringBuilder sb = new StringBuilder();
                string query = "SELECT ID_Uff3, SgUff3, DescUff3 FROM Liv3Uff WHERE ID_Uff1=@idUff1 AND ID_Uff2=@idUff2 ORDER BY SgUff3";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@idUff1", idUff1);
                    cmd.Parameters.AddWithValue("@idUff2", idUff2);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int idUff3 = Convert.ToInt32(dr["ID_Uff3"]);
                            string nome = $"{dr["SgUff3"]} - {dr["DescUff3"]}";
                            sb.Append($"<div class='ufficio' style='margin-left:{livello * 25}px'>");
                            sb.Append($"<div class='titolo'>{nome}</div>");

                            string resp3 = GetResponsabile(con, idUff1, idUff2, idUff3);
                            if (!string.IsNullOrEmpty(resp3))
                                sb.Append($"<div class='responsabile'>Responsabile: {resp3}</div>");
                        // ---------------- aggiungi qui gli addetti per Liv3 ----------------
                        string addettiLiv3 = GetAddetti(con, idUff1, idUff2, idUff3);
                        if (!string.IsNullOrEmpty(addettiLiv3))
                            sb.Append($"<div class='addetti'>Addetti: {addettiLiv3}</div>");
                        // -------------------------------------------------------------------

                        sb.Append("</div>");
                        }
                    }
                }
                return sb.ToString();
            }

            private string GetResponsabile(SqlConnection con, int? idUff1, int? idUff2, int? idUff3)
            {
                string query = @"
                SELECT TOP 1 e.Cognome + ' ' + e.Nome AS Responsabile
FROM Incarichi i
INNER JOIN ElencoPersonale e ON e.IDPersonale = i.IDPersonale
INNER JOIN Tipo_incarichi t ON t.id_tipo_incarico = i.id_tipo_incarico
WHERE i.principale = 1
  AND e.Stato_Servizio = 'Attivo'
  AND (@idUff1 IS NULL OR i.ID_Uff1 = @idUff1)
  AND (@idUff2 IS NULL OR i.ID_Uff2 = @idUff2)
  AND (@idUff3 IS NULL OR i.ID_Uff3 = @idUff3)
ORDER BY t.livello DESC, i.Data_inizio DESC
";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@idUff1", (object)idUff1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idUff2", (object)idUff2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idUff3", (object)idUff3 ?? DBNull.Value);
                    object result = cmd.ExecuteScalar();
                    return result == null ? "" : result.ToString();
                }
            }
        private string GetAddetti(SqlConnection con, int? idUff1, int? idUff2, int? idUff3)
        {
            string query = @"
        SELECT DISTINCT (e.Cognome + ' ' + e.Nome) AS Addetto
        FROM Incarichi i
        INNER JOIN ElencoPersonale e ON e.IDPersonale = i.IDPersonale
        WHERE i.id_tipo_incarico IN (38,45)
          AND e.Stato_Servizio = 'Attivo'
          AND (@idUff1 IS NULL OR i.ID_Uff1 = @idUff1)
          AND (@idUff2 IS NULL OR i.ID_Uff2 = @idUff2)
          AND (@idUff3 IS NULL OR i.ID_Uff3 = @idUff3)
        ORDER BY Addetto;
    ";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@idUff1", (object)idUff1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUff2", (object)idUff2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUff3", (object)idUff3 ?? DBNull.Value);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    StringBuilder sb = new StringBuilder();
                    bool hasAny = false;

                    while (dr.Read())
                    {
                        if (!hasAny)
                        {
                            sb.Append("<div class='addetti'>Addetti: ");
                            hasAny = true;
                        }

                        string addetto = dr["Addetto"] == DBNull.Value ? "" : dr["Addetto"].ToString();
                        sb.Append($"<span class='addetto'>{Server.HtmlEncode(addetto)}</span>");
                    }

                    if (hasAny)
                        sb.Append("</div>");

                    return sb.ToString(); // vuoto se non ci sono addetti
                }
            }
        }


    }
}

