<!-- Organigramma.aspx -->
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Organigramma4.aspx.cs" Inherits="DipendentiWeb.Organigramma4" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<title>Organigramma - DipendentiDB</title>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" crossorigin="anonymous" referrerpolicy="no-referrer" />
<style>
:root{--accent:#1f6feb;--muted:#6b7280}
body{font-family:Segoe UI, Arial, sans-serif;margin:24px;background:#f6f8fb;color:#0f172a}
.org-container{max-width:1100px;margin:0 auto}
h1{color:var(--accent);border-bottom:3px solid var(--accent);padding-bottom:8px}
.tree{list-style:none;padding-left:0}
.node{margin:6px 0;padding:8px 10px;border-radius:6px;background:#fff;box-shadow:0 1px 3px rgba(2,6,23,0.06);display:flex;align-items:center;gap:10px}
.node .meta{flex:1}
.node .title{font-weight:600;color:#0b2545;cursor:pointer}
.node .sub {
    font-weight: 600;
    color: #0b2545;
    font-size: 15px; /* più grande e più scuro */
}

.node .addetti {
    color: var(--muted);
    font-size: 13px;
    margin-top: 2px;
}
/* Titolo Addetti */
.addetti-title {
    font-weight: 600; /* Rendi "Addetti:" più visibile */
    margin-bottom: 2px;
}

/* Contenitore della lista degli addetti */
.addetti-list {
    display: flex;
    flex-direction: column; /* FORZA GLI ELEMENTI AD ANDARE IN COLONNA */
    gap: 1px; /* Spazio ridotto tra i nomi */
}

/* Singolo elemento addetto */
.addetto-item {
    line-height: 1.3;
}
.addetto-item a {
    color: var(--muted); /* Usa il colore smorzato per i link */
    text-decoration: none; /* Rimuove la sottolineatura */
}
.addetto-item a:hover {
    text-decoration: underline;
    color: var(--accent);
}
.toggle{width:28px;height:28px;display:inline-flex;align-items:center;justify-content:center;border-radius:6px;background:#eef4ff;color:var(--accent);cursor:pointer}
.children{margin-left:28px;padding-left:8px;border-left:2px dashed rgba(15,23,42,0.04)}
.collapsed > .children{display:none}
.search{margin:12px 0;display:flex;gap:8px}
.search input{flex:1;padding:8px 10px;border:1px solid #d1d5db;border-radius:6px}
.highlight{background:linear-gradient(90deg,#fffbeb,#fff1c2);}
@media (max-width:600px){.node{flex-direction:column;align-items:flex-start}}
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="org-container">
<h1>Organigramma</h1>
<div class="search">
<input id="txtSearch" type="search" placeholder="Cerca nome o ufficio (premi Enter)" />
<button type="button" id="btnClear">Clear</button>
</div>

<!-- Render area -->
<asp:Literal ID="litOrganigramma" runat="server" />



</div>
</form>

<script src="https://code.jquery.com/jquery-3.6.4.min.js"></script>
<script src="https://code.jquery.com/jquery-3.6.4.min.js"></script>
<script>
    (function ($) {
        $(function () {

            // Inizializza toggle per ogni .node che ha .children (sia come next sibling sia come child)
            $('.node').each(function () {
                var $node = $(this);

                // prova a trovare children sia come next che come descendant diretto
                var $children = $node.next('.children');
                if (!$children.length) {
                    // cerca children figlio diretto o primo discendente
                    $children = $node.children('.children').first();
                    if (!$children.length) {
                        $children = $node.find('> .children').first();
                    }
                    if (!$children.length) {
                        $children = $node.find('.children').first();
                    }
                }

                if ($children.length) {
                    var $toggle = $('<span class="toggle" role="button" aria-expanded="true" title="Espandi/Comprimi"><i class="fa fa-chevron-down" aria-hidden="true"></i></span>');
                    $node.prepend($toggle);

                    // click handler
                    $toggle.on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();

                        var expanded = $(this).attr('aria-expanded') === 'true';
                        $(this).attr('aria-expanded', !expanded);
                        $(this).find('i').toggleClass('fa-chevron-down fa-chevron-right');

                        // toggle della classe collapsed sul nodo
                        $node.toggleClass('collapsed');

                        // slide toggle sul container dei figli (funziona sia se è next che child)
                        $children.stop(true, true).slideToggle(180);
                    });
                } else {
                    // spazio per allineamento se non ci sono figli
                    $node.prepend('<span style="width:34px;display:inline-block"></span>');
                }
            });

            // Click sul titolo → apre/chiude il nodo (compatibile con le due strutture)
            $(document).on('click', '.node .title', function (e) {
                var $node = $(this).closest('.node');
                var $toggle = $node.children('.toggle').first();
                if ($toggle.length) {
                    $toggle.trigger('click');
                }
            });

            // --- Ricerca ---
            function clearHighlights() {
                $('.highlight').removeClass('highlight');
            }

            $('#txtSearch').on('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    var q = $(this).val().trim().toLowerCase();
                    clearHighlights();
                    if (!q) return;

                    var matched = false;
                    $('.node').each(function () {
                        var text = $(this).text().toLowerCase();
                        if (text.indexOf(q) !== -1) {
                            matched = true;
                            $(this).addClass('highlight');

                            // espandi tutti i genitori per mostrare il match
                            $(this).parents('.children').each(function () {
                                var $ch = $(this);

                                // se children è figlio del nodo (struttura "children inside node"),
                                // il parent node è il nodo stesso; altrimenti è il prev('.node').
                                var $parentNode = $ch.prev('.node');
                                if (!$parentNode.length) {
                                    $parentNode = $ch.closest('.node');
                                }

                                // mostra il container figli
                                $ch.show();

                                // rimuovi collapsed e aggiorna icona/aria-expanded se presente il toggle
                                $parentNode.removeClass('collapsed');
                                var $t = $parentNode.children('.toggle').first();
                                if ($t.length) {
                                    $t.attr('aria-expanded', true);
                                    $t.find('i').removeClass('fa-chevron-right').addClass('fa-chevron-down');
                                }
                            });

                            // scroll al risultato
                            $('html,body').animate({ scrollTop: $(this).offset().top - 80 }, 250);
                        }
                    });

                    if (!matched) {
                        console.log('Nessun risultato per: ' + q);
                    }
                }
            });

            $('#btnClear').on('click', function () {
                $('#txtSearch').val('');
                clearHighlights();
            });

        });
    })(jQuery);
</script>



</body>
</html>
