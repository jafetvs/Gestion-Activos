// Scripts/Views/Detalle_Adjuntos.js
(function () {
    // Helpers
    function qs(id) { return document.getElementById(id); }
    function val(id) { var el = qs(id); return el ? el.value : ""; }

    // Estado de paginación actual
    var currentPage = 1;

    // Cargar adjuntos con filtro + página
    function cargarAdjuntos(page) {
        currentPage = page || 1;

        var idActivo = (window.ADJ_CFG && window.ADJ_CFG.idActivo)
            ? window.ADJ_CFG.idActivo
            : val("hdnIdActivo");

        var q = $("#txtBuscarAdj").val() || ""; // filtro por nombre

        var url = window.ADJ_CFG.routes.list
            + "?idActivo=" + encodeURIComponent(idActivo)
            + "&texto=" + encodeURIComponent(q)
            + "&q=" + encodeURIComponent(q)  // alias por compatibilidad
            + "&page=" + currentPage
            + "&_=" + Date.now();            // anti-cache

        $("#zona-adjuntos").load(url);
    }

    // Exponer para paginación desde el partial
    window.ad_irPagina = function (p) { cargarAdjuntos(p); };

    // Exponer recarga conservando página actual (para usar tras guardar/renombrar)
    window.ad_reload = function () { cargarAdjuntos(currentPage); };

    // Abrir modal (crear o renombrar)
    //  - Para crear: { idAdjunto: 0, idActivo }
    //  - Para renombrar: { idAdjunto, idActivo }
    window.abrirModalAdjunto = function (opt) {
        var url = window.ADJ_CFG.routes.form
            + "?idActivo=" + encodeURIComponent(opt.idActivo)
            + "&idAdjunto=" + encodeURIComponent(opt.idAdjunto || 0)
            + "&_=" + Date.now();

        $("#modalAdjunto .modal-content").load(url, function () {
            new bootstrap.Modal(document.getElementById("modalAdjunto")).show();
        });
    };

    // Eliminar
    window.eliminarAdjunto = function (idAdjunto) {
        if (!confirm("¿Eliminar adjunto?")) return;

        $.ajax({
            type: "POST",
            url: window.ADJ_CFG.routes.del,
            data: {
                idAdjunto: idAdjunto,
                idActivo: val("hdnIdActivo"),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }
        }).done(function (res) {
            if (res && res.ok) {
                // recarga manteniendo página actual
                window.ad_reload();
            } else {
                alert((res && res.msg) || "No se pudo eliminar.");
            }
        }).fail(function () {
            alert("Error de red al eliminar.");
        });
    };

    // Descargar (si alguna vez quieres hacerlo por JS en lugar del href directo)
    window.descargarAdjunto = function (idAdjunto) {
        // Lo normal es usar el <a href="..."> directo en la tabla.
        // Pero si lo necesitas por JS:
        var url = window.ADJ_CFG.routes.download + "?idAdjunto=" + encodeURIComponent(idAdjunto);
        window.location.href = url;
    };

    // Cargar cuando se muestra la pestaña de Adjuntos
    document.addEventListener("shown.bs.tab", function (ev) {
        if (ev.target && ev.target.id === "tab-adj") {
            cargarAdjuntos(1);
        }
    });

    // Búsqueda en vivo (debounce)
    var timer = null;
    function liveReload() {
        clearTimeout(timer);
        timer = setTimeout(function () { cargarAdjuntos(1); }, 250);
    }

    // Eventos
    $(document).ready(function () {
        // Si abriste Detalle con #adjuntos en el hash, carga de una
        if ((location.hash || "").toLowerCase() === "#adjuntos") {
            cargarAdjuntos(1);
        }

        // Botón Buscar
        $("#btnBuscarAdj").off("click.adj").on("click.adj", function () {
            cargarAdjuntos(1);
        });

        // Texto: input/keyup/paste/change en vivo
        $(document)
            .off("input.adj keyup.adj paste.adj change.adj", "#txtBuscarAdj")
            .on("input.adj keyup.adj paste.adj change.adj", "#txtBuscarAdj", liveReload);

        // Enter como “buscar ya”
        $(document)
            .off("keydown.adj", "#txtBuscarAdj")
            .on("keydown.adj", "#txtBuscarAdj", function (e) {
                if (e.which === 13) { e.preventDefault(); cargarAdjuntos(1); }
            });

        // Limpiar filtro (solo texto)
        $("#btnLimpiarAdj").off("click.adj").on("click.adj", function () {
            $("#txtBuscarAdj").val("");
            cargarAdjuntos(1);
        });

        // Nuevo adjunto (subir)
        $("#btnNuevoAdj").off("click.adj").on("click.adj", function () {
            abrirModalAdjunto({ idAdjunto: 0, idActivo: val("hdnIdActivo") });
        });
    });
})();
