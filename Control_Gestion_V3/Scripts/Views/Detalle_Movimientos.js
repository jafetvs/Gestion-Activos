(function () {
    function qs(id) { return document.getElementById(id); }
    function val(id) { var el = qs(id); return el ? el.value : ""; }

    function cargarMovimientos(page) {
        page = page || 1;

        var idActivo = val("hdnIdActivo");
        var desde = val("fDesde");
        var hasta = val("fHasta");
        var q = $("#txtBuscarMov").val() || "";

        var url = window.MOV_CFG.routes.list
            + "?idActivo=" + encodeURIComponent(idActivo)
            + "&desde=" + encodeURIComponent(desde)
            + "&hasta=" + encodeURIComponent(hasta)
            + "&q=" + encodeURIComponent(q)
            + "&texto=" + encodeURIComponent(q)
            + "&page=" + page
            + "&_=" + Date.now();

        $("#zona-movimientos").load(url);
    }

    // Cargar cuando se muestra la pestaña de Movimientos
    document.addEventListener("shown.bs.tab", function (ev) {
        if (ev.target && ev.target.id === "tab-mov") cargarMovimientos();
    });

    document.addEventListener("DOMContentLoaded", function () {
        if ((location.hash || "").toLowerCase() === "#movimientos") cargarMovimientos();

        // Botón buscar (sigue funcionando)
        $("#btnBuscarMov").off("click.mov").on("click.mov", function () {
            cargarMovimientos(1);
        });

        // >>> BÚSQUEDA EN VIVO <<<
        // Dispara al escribir, pegar o cambiar (sin condición de Enter)
        // Namespaces (.mov) para evitar duplicados si el script se vuelve a cargar.
        $(document)
            .off("input.mov keyup.mov paste.mov change.mov", "#txtBuscarMov")
            .on("input.mov keyup.mov paste.mov change.mov", "#txtBuscarMov", function () {
                cargarMovimientos(1);
            });

        // Si quieres mantener Enter como “buscar ya mismo”, no hace falta condicional
        // porque arriba ya dispara en cada tecla, pero lo dejo por claridad:
        $(document)
            .off("keydown.mov", "#txtBuscarMov")
            .on("keydown.mov", "#txtBuscarMov", function (e) {
                if (e.which === 13) { e.preventDefault(); cargarMovimientos(1); }
            });

        // Fechas: recarga automático al cambiar
        $("#fDesde,#fHasta").off("change.mov").on("change.mov", function () {
            cargarMovimientos(1);
        });

        // Limpiar filtros
        $("#btnLimpiarMov").off("click.mov").on("click.mov", function () {
            $("#txtBuscarMov").val(""); $("#fDesde").val(""); $("#fHasta").val("");
            cargarMovimientos(1);
        });

        // Nuevo movimiento
        $("#btnNuevoMov").off("click.mov").on("click.mov", function () {
            abrirModalMovimiento({ idMovimiento: 0, idActivo: val("hdnIdActivo") });
        });
    });

    // Abrir modal crear/editar
    window.abrirModalMovimiento = function (opt) {
        var url = window.MOV_CFG.routes.form
            + "?idActivo=" + encodeURIComponent(opt.idActivo)
            + "&idMovimiento=" + encodeURIComponent(opt.idMovimiento || 0)
            + "&_=" + Date.now();

        $("#modalMovimiento .modal-content").load(url, function () {
            new bootstrap.Modal(document.getElementById("modalMovimiento")).show();
        });
    };

    // Eliminar movimiento
    window.eliminarMovimiento = function (idMovimiento) {
        if (!confirm("¿Eliminar movimiento?")) return;

        $.ajax({
            type: "POST",
            url: window.MOV_CFG.routes.del,
            data: {
                idMovimiento: idMovimiento,
                idActivo: val("hdnIdActivo"),
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }
        }).done(function (res) {
            if (res && res.ok) cargarMovimientos();
            else alert((res && res.msg) || "No se pudo eliminar.");
        }).fail(function () {
            alert("Error de red al eliminar.");
        });
    };

    // Paginación (la usa el partial)
    window.irPagina = function (p) { cargarMovimientos(p); };
})();
