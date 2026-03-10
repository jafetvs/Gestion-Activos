(function () {
    // Helpers
    function qs(id) { return document.getElementById(id); }
    function val(id) { var el = qs(id); return el ? el.value : ""; }

    // Estado de paginación actual
    var currentPage = 1;

    // Cargar mantenimientos con filtros + página
    function cargarMantenimientos(page) {
        currentPage = page || 1;

        var idActivo = (window.MNT_CFG && window.MNT_CFG.idActivo) ? window.MNT_CFG.idActivo : val("hdnIdActivo");

        var desde = val("fMnDesde");              // yyyy-MM-dd o ""
        var hasta = val("fMnHasta");              // yyyy-MM-dd o ""
        var tipo = val("mnTipoFiltro");          // "", "Preventivo", etc.
        var q = $("#txtBuscarMnt").val() || "";

        var url = window.MNT_CFG.routes.list
            + "?idActivo=" + encodeURIComponent(idActivo)
            + "&desde=" + encodeURIComponent(desde)
            + "&hasta=" + encodeURIComponent(hasta)
            + "&tipo=" + encodeURIComponent(tipo)
            + "&texto=" + encodeURIComponent(q)
            + "&q=" + encodeURIComponent(q)   // alias
            + "&t=" + encodeURIComponent(tipo)// alias
            + "&page=" + currentPage
            + "&_=" + Date.now();             // anti-cache

        $("#zona-mantenimientos").load(url);
    }

    // Exponer para paginación desde el partial
    window.mnt_irPagina = function (p) { cargarMantenimientos(p); };

    // Exponer recarga conservando página actual (para usar tras guardar)
    window.mnt_reload = function () { cargarMantenimientos(currentPage); };

    // Abrir modal crear/editar
    window.abrirModalMantenimiento = function (opt) {
        var url = window.MNT_CFG.routes.form
            + "?idActivo=" + encodeURIComponent(opt.idActivo)
            + "&idMantenimiento=" + encodeURIComponent(opt.idMantenimiento || 0)
            + "&_=" + Date.now();

        $("#modalMantenimiento .modal-content").load(url, function () {
            new bootstrap.Modal(document.getElementById("modalMantenimiento")).show();
        });
    };

    // Eliminar
    window.eliminarMantenimiento = function (idMantenimiento) {
        if (!confirm("¿Eliminar mantenimiento?")) return;

        $.ajax({
            type: "POST",
            url: window.MNT_CFG.routes.del,
            data: {
                idMantenimiento: idMantenimiento,
                idActivo: val("hdnIdActivo"),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }
        }).done(function (res) {
            if (res && res.ok) {
                // recarga manteniendo página actual
                window.mnt_reload();
            } else {
                alert((res && res.msg) || "No se pudo eliminar.");
            }
        }).fail(function () {
            alert("Error de red al eliminar.");
        });
    };

    // Cargar cuando se muestra la pestaña de Mantenimientos
    document.addEventListener("shown.bs.tab", function (ev) {
        if (ev.target && ev.target.id === "tab-mnt") {
            cargarMantenimientos(1);
        }
    });

    // Búsqueda en vivo (debounce)
    var timer = null;
    function liveReload() {
        clearTimeout(timer);
        timer = setTimeout(function () { cargarMantenimientos(1); }, 250);
    }

    // Eventos
    $(document).ready(function () {
        // Si abriste Detalle con #mantenimientos en el hash, carga de una
        if ((location.hash || "").toLowerCase() === "#mantenimientos") {
            cargarMantenimientos(1);
        }

        // Botón Buscar
        $("#btnBuscarMnt").off("click.mnt").on("click.mnt", function () {
            cargarMantenimientos(1);
        });

        // Texto: input/keyup/paste/change en vivo
        $(document)
            .off("input.mnt keyup.mnt paste.mnt change.mnt", "#txtBuscarMnt")
            .on("input.mnt keyup.mnt paste.mnt change.mnt", "#txtBuscarMnt", liveReload);

        // Enter como “buscar ya”
        $(document)
            .off("keydown.mnt", "#txtBuscarMnt")
            .on("keydown.mnt", "#txtBuscarMnt", function (e) {
                if (e.which === 13) { e.preventDefault(); cargarMantenimientos(1); }
            });

        // Fechas: recarga al cambiar
        $("#fMnDesde,#fMnHasta").off("change.mnt").on("change.mnt", function () {
            cargarMantenimientos(1);
        });

        // Tipo: recarga al cambiar
        $("#mnTipoFiltro").off("change.mnt").on("change.mnt", function () {
            cargarMantenimientos(1);
        });

        // Limpiar filtros (texto, fechas, tipo)
        $("#btnLimpiarMnt").off("click.mnt").on("click.mnt", function () {
            $("#txtBuscarMnt").val("");
            $("#fMnDesde").val("");
            $("#fMnHasta").val("");
            $("#mnTipoFiltro").prop("selectedIndex", 0); // primer item "(Todos)"
            cargarMantenimientos(1);
        });

        // Nuevo mantenimiento
        $("#btnNuevoMnt").off("click.mnt").on("click.mnt", function () {
            abrirModalMantenimiento({ idMantenimiento: 0, idActivo: val("hdnIdActivo") });
        });
    });
})();
