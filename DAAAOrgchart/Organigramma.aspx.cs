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
            sb.Append("<ul class='tree'>");

            using (SqlConnection con = new SqlConnection(connString))
            {
                con.Open();

                // DIRETTORE (nodo principale)
                sb.Append("<li>");
                sb.Append(GetUfficioPrincipale(con, 11, "Direttore"));

                // --- Prime linee come figli del Direttore ---
                sb.Append("<div class='children'>");

                string queryPrimeLinee = @"
            SELECT ID_Uff1, SgUff1, DescUff1, Livello
            FROM Liv1Uff
            WHERE ID_Uff1 <> 11
            ORDER BY 
                CASE WHEN Livello = 0 THEN 999 ELSE Livello END ASC,
                SgUff1 ASC";

                using (SqlCommand cmd = new SqlCommand(queryPrimeLinee, con))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int idUff1 = Convert.ToInt32(dr["ID_Uff1"]);
                        string nome = $"{dr["SgUff1"]} - {dr["DescUff1"]}";

                        sb.Append("<div class='node' data-id1='" + idUff1 + "'>");
                        sb.Append("<div class='meta'>");
                        sb.Append("<div class='title'>" + System.Web.HttpUtility.HtmlEncode(nome) + "</div>");

                        string resp = GetResponsabile(con, idUff1, null, null);
                        if (!string.IsNullOrEmpty(resp))
                            sb.Append($"<div class='sub'>{System.Web.HttpUtility.HtmlEncode(resp)}</div>");


                        string addetti = GetAddetti(con, idUff1, null, null);
                        if (!string.IsNullOrEmpty(addetti))
                            sb.Append(addetti);

                        sb.Append("</div>"); // meta
                        sb.Append("</div>"); // node

                        // figli (Liv2 e Liv3)
                        sb.Append(GetLiv2(con, idUff1, 1));
                    }
                }

                sb.Append("</div>"); // chiude children del Direttore
                sb.Append("</li>");
            }

            sb.Append("</ul>");
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
                        // Node
                        sb.Append($"<div class='node' data-id1='{idUff1}'>");
                        sb.Append("<div class='meta'>");
                        //sb.Append($"<div class='title'>{System.Web.HttpUtility.HtmlEncode(titolo + \": \" + nome)}</div>");
                        sb.Append("<div class='title'>" + System.Web.HttpUtility.HtmlEncode(titolo + ": " + nome) + "</div>");

                        string resp = GetResponsabile(con, idUff1, null, null);
                        if (!string.IsNullOrEmpty(resp))                            
                            sb.Append($"<div class='sub'>{System.Web.HttpUtility.HtmlEncode(resp)}</div>");

                        string addetti = GetAddetti(con, idUff1, null, null);
                        if (!string.IsNullOrEmpty(addetti))
                            sb.Append(addetti);
                        sb.Append("</div>"); // meta
                        sb.Append("</div>"); // node

                        // children (es. Liv2 per il principale)
                        sb.Append(GetLiv2(con, idUff1, 1));
                    }
                }
            }

            return sb.ToString();
        }
        private string GetLiv2(SqlConnection con, int idUff1, int livello)
        {
            StringBuilder sb = new StringBuilder();

            //string query = "SELECT ID_Uff2, SgUff2, DescUff2 FROM Liv2Uff WHERE ID_Uff1=@idUff1 ORDER BY SgUff2";
            string query = @"
                            SELECT ID_Uff2, SgUff2, DescUff2, Livello
                            FROM Liv2Uff
                            WHERE ID_Uff1 = @idUff1
                            ORDER BY 
                                CASE WHEN Livello = 0 THEN 999 ELSE Livello END ASC,
                                SgUff2 ASC";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@idUff1", idUff1);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.HasRows)
                        return ""; // ❌ nessun figlio → niente <div class='children'>

                    sb.Append("<div class='children'>");
                    while (dr.Read())
                    {
                        int idUff2 = Convert.ToInt32(dr["ID_Uff2"]);
                        string nome = $"{dr["SgUff2"]} - {dr["DescUff2"]}";

                        sb.Append($"<div class='node' data-id1='{idUff1}' data-id2='{idUff2}'>");
                        sb.Append("<div class='meta'>");
                        sb.Append($"<div class='title'>{System.Web.HttpUtility.HtmlEncode(nome)}</div>");

                        string resp2 = GetResponsabile(con, idUff1, idUff2, null);
                        if (!string.IsNullOrEmpty(resp2))
                            sb.Append($"<div class='sub'>{System.Web.HttpUtility.HtmlEncode(resp2)}</div>");                        



                        // ✅ addetti solo se diretti all'ufficio
                        string addettiLiv2 = GetAddetti(con, idUff1, idUff2, null);
                        if (!string.IsNullOrEmpty(addettiLiv2))
                            sb.Append(addettiLiv2);

                        sb.Append("</div></div>"); // meta + node

                        // ricorsione verso Liv3
                        string liv3 = GetLiv3(con, idUff1, idUff2, livello + 1);
                        if (!string.IsNullOrEmpty(liv3))
                            sb.Append(liv3);
                    }
                    sb.Append("</div>");
                }
            }

            return sb.ToString();
        }

        private string GetLiv3(SqlConnection con, int idUff1, int idUff2, int livello)
        {
            StringBuilder sb = new StringBuilder();

            //string query = "SELECT ID_Uff3, SgUff3, DescUff3 FROM Liv3Uff WHERE ID_Uff1=@idUff1 AND ID_Uff2=@idUff2 ORDER BY SgUff3";
            string query = @"
                            SELECT ID_Uff3, SgUff3, DescUff3, Livello
                            FROM Liv3Uff
                            WHERE ID_Uff1 = @idUff1 AND ID_Uff2 = @idUff2
                            ORDER BY 
                                CASE WHEN Livello = 0 THEN 999 ELSE Livello END ASC,
                                SgUff3 ASC";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@idUff1", idUff1);
                cmd.Parameters.AddWithValue("@idUff2", idUff2);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.HasRows)
                        return ""; // ❌ nessun figlio → niente <div class='children'>

                    sb.Append("<div class='children'>");
                    while (dr.Read())
                    {
                        int idUff3 = Convert.ToInt32(dr["ID_Uff3"]);
                        string nome = $"{dr["SgUff3"]} - {dr["DescUff3"]}";

                        sb.Append($"<div class='node' data-id1='{idUff1}' data-id2='{idUff2}' data-id3='{idUff3}'>");
                        sb.Append("<div class='meta'>");
                        sb.Append($"<div class='title'>{System.Web.HttpUtility.HtmlEncode(nome)}</div>");

                        string resp3 = GetResponsabile(con, idUff1, idUff2, idUff3);
                        if (!string.IsNullOrEmpty(resp3))
                            sb.Append($"<div class='sub'>{System.Web.HttpUtility.HtmlEncode(resp3)}</div>");


                        string addettiLiv3 = GetAddetti(con, idUff1, idUff2, idUff3);
                        if (!string.IsNullOrEmpty(addettiLiv3))
                            sb.Append(addettiLiv3);

                        sb.Append("</div></div>");
                    }
                    sb.Append("</div>");
                }
            }

            return sb.ToString();
        }

        private string GetResponsabile(SqlConnection con, int? idUff1, int? idUff2, int? idUff3)
        {
            // Normalizza valori speciali a NULL
            idUff1 = (idUff1 == 22) ? null : idUff1;
            idUff2 = (idUff2 == 119) ? null : idUff2;
            idUff3 = (idUff3 == 195) ? null : idUff3;

            string baseQuery = @"
        SELECT TOP 1 
            e.Cognome + ' ' + e.Nome AS NomeCompleto,
            CASE 
                WHEN e.Militare = 1 THEN ISNULL(g.SiglaGrado, '')
                ELSE ISNULL(t.Sigla_titolo, '')
            END AS Qualifica,
            ti.Descr_incarico,
            ti.livello
        FROM Incarichi i
        INNER JOIN ElencoPersonale e ON e.IDPersonale = i.IDPersonale
        INNER JOIN Tipo_incarichi ti ON i.id_tipo_incarico = ti.id_tipo_incarico
        LEFT JOIN Profilo_militare pm ON e.IDPersonale = pm.IDPersonale
        LEFT JOIN Gradi g ON pm.ID_Grado = g.ID_Grado
        LEFT JOIN PersCivile pc ON e.IDPersonale = pc.IDPersonale
        LEFT JOIN Titoli t ON pc.ID_TitoloAtt = t.ID_Titolo
        WHERE e.Stato_Servizio = 'Attivo'
          AND ti.livello >= 60
          AND (
                (i.ID_Uff3 = @idUff3 OR (@idUff3 IS NULL AND (i.ID_Uff3 IS NULL OR i.ID_Uff3 = 195)))
             AND (i.ID_Uff2 = @idUff2 OR (@idUff2 IS NULL AND (i.ID_Uff2 IS NULL OR i.ID_Uff2 = 119)))
             AND (i.ID_Uff1 = @idUff1 OR (@idUff1 IS NULL AND (i.ID_Uff1 IS NULL OR i.ID_Uff1 = 22)))
          )
          AND i.principale = @principale
        ORDER BY ti.livello DESC, i.Data_inizio DESC";

            // 1° tentativo: responsabile principale
            string responsabile = EseguiQueryResponsabile(con, baseQuery, idUff1, idUff2, idUff3, true);

            // 2° tentativo: responsabile secondario
            if (string.IsNullOrEmpty(responsabile))
                responsabile = EseguiQueryResponsabile(con, baseQuery, idUff1, idUff2, idUff3, false);

            return responsabile ?? string.Empty;
        }
        private string EseguiQueryResponsabile(SqlConnection con, string query, int? idUff1, int? idUff2, int? idUff3, bool principale)
        {
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@idUff1", (object)idUff1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUff2", (object)idUff2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUff3", (object)idUff3 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@principale", principale);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string nome = dr["NomeCompleto"]?.ToString() ?? "";
                        string qualifica = dr["Qualifica"]?.ToString() ?? "";
                        string descrIncarico = dr["Descr_incarico"]?.ToString() ?? "";
                        // anteponi grado/titolo al nome
                        return $"{descrIncarico} - {qualifica} {nome} ";
                        

                    }
                }
            }
            return string.Empty;
        }

        private string GetAddetti(SqlConnection con, int? idUff1, int? idUff2, int? idUff3)
        {
            // Normalizza valori speciali a NULL
            idUff1 = (idUff1 == 22) ? null : idUff1;
            idUff2 = (idUff2 == 119) ? null : idUff2;
            idUff3 = (idUff3 == 195) ? null : idUff3;

            string query = @"
        SELECT DISTINCT 
            e.Cognome + ' ' + e.Nome AS Addetto,
            CASE 
                WHEN e.Militare = 1 THEN ISNULL(g.SiglaGrado, '')
                ELSE ISNULL(t.Sigla_titolo, '')
            END AS Qualifica
        FROM Incarichi i
        INNER JOIN ElencoPersonale e ON e.IDPersonale = i.IDPersonale
        LEFT JOIN Profilo_militare pm ON e.IDPersonale = pm.IDPersonale
        LEFT JOIN Gradi g ON pm.ID_Grado = g.ID_Grado
        LEFT JOIN PersCivile pc ON e.IDPersonale = pc.IDPersonale
        LEFT JOIN Titoli t ON pc.ID_TitoloAtt = t.ID_Titolo
        WHERE i.id_tipo_incarico IN (38, 45)
          AND e.Stato_Servizio = 'Attivo'
          AND (
                (i.ID_Uff3 = @idUff3 OR (@idUff3 IS NULL AND (i.ID_Uff3 IS NULL OR i.ID_Uff3 = 195)))
             AND (i.ID_Uff2 = @idUff2 OR (@idUff2 IS NULL AND (i.ID_Uff2 IS NULL OR i.ID_Uff2 = 119)))
             AND (i.ID_Uff1 = @idUff1 OR (@idUff1 IS NULL AND (i.ID_Uff1 IS NULL OR i.ID_Uff1 = 22)))
          )
        ORDER BY Addetto";

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
                        string addetto = dr["Addetto"]?.ToString() ?? "";
                        string qualifica = dr["Qualifica"]?.ToString() ?? "";
                        // anteponi grado/titolo al nome
                        sb.Append($"<span class='addetto'>{Server.HtmlEncode(qualifica)} {Server.HtmlEncode(addetto)}</span> ");
                    }
                    if (hasAny) sb.Append("</div>");
                    return sb.ToString();
                }
            }
        }




    }
}

