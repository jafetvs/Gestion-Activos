// === Utilidades comunes ===
(function () {

    // ---- Helpers ----
    function getQueryParam(name) {
        try {
            const url = new URL(window.location.href);
            return url.searchParams.get(name);
        } catch (_) { return null; }
    }

    function buildQuery(params) {
        const usp = new URLSearchParams();
        Object.keys(params).forEach(k => {
            const v = params[k];
            if (v !== null && v !== undefined && v !== '') {
                usp.set(k, v);
            }
        });
        return usp.toString();
    }

    function debounce(fn, ms) {
        let t = null;
        return function () {
            const ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, ms);
        };
    }

    // Intenta tomar las URLs desde $.MisUrls si existe; si no, usa rutas por defecto
    function getUrlEliminar() {
        if (window.$ && $.MisUrls && $.MisUrls.url && $.MisUrls.url._EliminarActivo) {
            return $.MisUrls.url._EliminarActivo;
        }
        return '/Activos/Eliminar';
    }

    function getUrlFiltrar() {
        if (window.$ && $.MisUrls && $.MisUrls.url && $.MisUrls.url._FiltrarActivos) {
            return $.MisUrls.url._FiltrarActivos;
        }
        return '/Activos/Filtrar';
    }

    // ---- Soft-delete (Eliminar = Desechado) ----
    function bindEliminar() {
        var filtroClasif = getQueryParam('clasificacion');
        var mostrandoDesechados = (filtroClasif && filtroClasif.toLowerCase() === 'desechado');

        document.querySelectorAll('.btn-eliminar').forEach(function (btn) {
            btn.removeEventListener('click', btn._eliminarHandler, false);

            btn._eliminarHandler = function () {
                var id = this.getAttribute('data-id');
                if (!id) return;

                if (!confirm('¿Desea marcar este activo como "Desechado"?')) return;

                fetch(getUrlEliminar(), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                    body: 'id=' + encodeURIComponent(id)
                })
                    .then(r => r.json())
                    .then(json => {
                        if (json.ok) {
                            var tr = document.querySelector('tr[data-id="' + id + '"]');
                            if (!tr) return;

                            if (mostrandoDesechados) {
                                var celdaClasif = tr.querySelector('td.celda-clasif') || tr.querySelector('td:nth-child(5)');
                                if (celdaClasif) celdaClasif.textContent = 'Desechado';
                                alert(json.msg || 'Activo marcado como Desechado.');
                            } else {
                                tr.parentNode.removeChild(tr);
                            }
                        } else {
                            alert(json.msg || 'No se pudo marcar como Desechado.');
                        }
                    })
                    .catch(_ => alert('Error al intentar cambiar clasificación.'));
            };

            btn.addEventListener('click', btn._eliminarHandler, false);
        });
    }

    // ---- Filtrado AJAX (actualiza solo el <tbody>) ----
    function bindFiltroAjax() {
        var frm = document.getElementById('frmFiltros');
        if (!frm) return;

        // Selects pueden estar FUERA del form (con form="frmFiltros")
        function pick(name) {
            return document.querySelector(
                '#frmFiltros [name="' + name + '"], [name="' + name + '"][form="frmFiltros"]'
            );
        }

        var txt = pick('texto');
        var ddlCat = pick('idCategoria');
        var ddlDep = pick('idDepto');
        var ddlCla = pick('clasificacion');
        var ddlTipo = pick('idTipo');   // NUEVO
        var ddlClase = pick('idClase');  // NUEVO

        var tbody = document.querySelector('#tabla-activos tbody');
        if (!tbody) return;

        function requestAjax() {
            var params = {
                texto: txt ? txt.value.trim() : '',
                idCategoria: ddlCat ? ddlCat.value : '',
                idDepto: ddlDep ? ddlDep.value : '',
                clasificacion: ddlCla ? ddlCla.value : '',
                idTipo: ddlTipo ? ddlTipo.value : '',     // NUEVO
                idClase: ddlClase ? ddlClase.value : ''   // NUEVO
            };

            console.log("Filtrando con:", params);  // dentro de requestAjax()
            console.log("URL:", url);

            var url = getUrlFiltrar() + '?' + buildQuery(params);

            fetch(url, { method: 'GET', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(r => r.text())
                .then(html => {
                    tbody.innerHTML = html;
                    bindEliminar();

                    // Refresca la URL para mantener los filtros (incluye tipo/clase)
                    var qs = buildQuery(params);
                    var base = window.location.pathname;
                    var newUrl = qs ? (base + '?' + qs) : base;
                    window.history.replaceState({}, '', newUrl);
                })
                .catch(_ => console.warn('Error al filtrar por AJAX'));
        }

        // Texto: en vivo con debounce
        if (txt) txt.addEventListener('input', debounce(requestAjax, 300));

        // Selects: al cambiar
        [ddlCat, ddlDep, ddlCla, ddlTipo, ddlClase].forEach(function (ddl) {
            if (ddl) ddl.addEventListener('change', requestAjax);
        });

        // Evita navegación completa si se presiona "Buscar"
        frm.addEventListener('submit', function (e) {
            e.preventDefault();
            requestAjax();
        });
    }

    // ---- Inicialización ----
    document.addEventListener('DOMContentLoaded', function () {
        bindEliminar();
        bindFiltroAjax();
    });

})();
