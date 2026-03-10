(function () {
    function qs(id) { return document.getElementById(id); }
    function val(id) { var el = qs(id); return el ? el.value : ""; }

    // ---- Debounce para búsqueda en vivo ----
    function debounce(fn, ms) {
        var t = null;
        return function () {
            var ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, ms);
        };
    }

    // ---- Cargar lista (con filtros) ----
    function cargarSoftware(page) {
        page = page || 1;

        var idActivo = val("hdnIdActivo");     // mismo hidden que usas en Detalle (pestaña Movimientos)
        var desde = val("swDesde");
        var hasta = val("swHasta");
        var q = $("#txtBuscarSw").val() || "";

        var url = window.SW_CFG.routes.list
            + "?idActivo=" + encodeURIComponent(idActivo)
            + "&desde=" + encodeURIComponent(desde)
            + "&hasta=" + encodeURIComponent(hasta)
            // Enviamos ambos por compatibilidad (Software y SoftwareLista)
            + "&q=" + encodeURIComponent(q)
            + "&texto=" + encodeURIComponent(q)
            + "&page=" + page
            + "&_=" + Date.now();

        $("#zona-software").load(url);
    }

    // ---- Al abrir la pestaña Software, carga ----
    document.addEventListener("shown.bs.tab", function (ev) {
        if (ev.target && ev.target.id === "tab-sw") cargarSoftware();
    });

    // ---- Init ----
    document.addEventListener("DOMContentLoaded", function () {
        // Si llegas con hash #software
        if ((location.hash || "").toLowerCase() === "#software") cargarSoftware();

        // Buscar (click)
        $("#btnBuscarSw").on("click", function () { cargarSoftware(1); });

        // Buscar en vivo (debounce + input)
        var input = document.getElementById("txtBuscarSw");
        if (input) {
            input.addEventListener("input", debounce(function () {
                cargarSoftware(1);
            }, 300));
        }

        // Rangos de fecha -> cambio inmediato
        $("#swDesde,#swHasta").on("change", function () { cargarSoftware(1); });

        // Limpiar filtros
        $("#btnLimpiarSw").on("click", function () {
            $("#txtBuscarSw").val(""); $("#swDesde").val(""); $("#swHasta").val("");
            cargarSoftware(1);
        });

        // Nuevo
        $("#btnNuevoSw").on("click", function () {
            abrirModalSoftware({ idSoftware: 0, idActivo: val("hdnIdActivo") });
        });
    });

    // ---- Modal (crear/editar) ----
    window.abrirModalSoftware = function (opt) {
        var url = window.SW_CFG.routes.form
            + "?idActivo=" + encodeURIComponent(opt.idActivo)
            + "&idSoftware=" + encodeURIComponent(opt.idSoftware || 0)
            + "&_=" + Date.now();

        $("#modalSoftware .modal-content").load(url, function () {
            new bootstrap.Modal(document.getElementById("modalSoftware")).show();
        });
    };

    // ---- Eliminar ----
    window.eliminarSoftware = function (idSoftware) {
        if (!confirm("¿Eliminar software?")) return;

        $.ajax({
            type: "POST",
            url: window.SW_CFG.routes.del,
            data: {
                idSoftware: idSoftware,
                idActivo: val("hdnIdActivo"),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }
        }).done(function (res) {
            if (res && res.ok) cargarSoftware();
            else alert((res && res.msg) || "No se pudo eliminar.");
        }).fail(function () {
            alert("Error de red al eliminar.");
        });
    };

    // ---- Paginación ----
    window.sw_irPagina = function (p) { cargarSoftware(p); };
})();
