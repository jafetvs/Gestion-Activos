(function () {
    const cfg = window.PROV_CFG;
    if (!cfg) return;

    const $form = document.getElementById('frmFiltros');
    const $tbody = document.getElementById('tbProveedores');
    const $btnLimpiar = document.getElementById('btnLimpiar');
    const $btnXlsx = document.getElementById('btnExportarExcel');
    const $btnPdf = document.getElementById('btnExportarPdf');
    const $msgHost = document.getElementById('provMsg');

    function showMsg(kind, text) {
        if (!$msgHost) return;
        $msgHost.innerHTML = `
            <div class="alert alert-${kind} alert-dismissible fade show" role="alert">
                ${text}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;
    }

    function qs(obj) {
        const p = new URLSearchParams();
        Object.keys(obj).forEach(k => {
            const v = obj[k];
            if (v !== null && v !== undefined && v !== '') p.append(k, v);
        });
        return p.toString();
    }

    function getFilters() {
        const fd = new FormData($form);
        return {
            texto: fd.get('texto') || '',
            servicio: fd.get('servicio') || '',
            noContrato: fd.get('noContrato') || '',
            noProcedimiento: fd.get('noProcedimiento') || '',
            renovacionContrato: fd.get('renovacionContrato') || '',
            activo: fd.get('activo') || '',
            venceDesde: fd.get('venceDesde') || '',
            venceHasta: fd.get('venceHasta') || ''
        };
    }

    async function cargar() {
        const params = getFilters();
        const url = cfg.routes.listar + '?' + qs(params);

        $tbody.innerHTML = '<tr><td class="text-muted small p-3" colspan="11">Cargando…</td></tr>';

        try {
            const res = await fetch(url, { credentials: 'same-origin' });
            const html = await res.text();
            $tbody.innerHTML = html;
            wireDeleteButtons();
        } catch (e) {
            console.error(e);
            $tbody.innerHTML = '<tr><td class="text-danger small p-3" colspan="11">Error al cargar.</td></tr>';
        }
    }

    function resetForm() {
        $form.reset();
        cargar();
    }

    function debounce(fn, ms) {
        let t;
        return function () {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, arguments), ms || 300);
        };
    }

    function wireDeleteButtons() {
        document.querySelectorAll('.btn-del').forEach(btn => {
            btn.onclick = async function () {
                const row = btn.closest('tr');
                const frm = btn.closest('form.frm-del');
                if (!row || !frm) return;

                if (!confirm('¿Eliminar este proveedor?')) return;

                try {
                    const fd = new FormData(frm); // incluye __RequestVerificationToken e id
                    const res = await fetch(cfg.routes.eliminar, {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest' // para Request.IsAjaxRequest()
                        },
                        body: fd
                    });

                    // Intentamos leer JSON (acción AJAX)
                    let data = null;
                    try { data = await res.json(); } catch (_) { }

                    if (data && typeof data.ok === 'boolean') {
                        if (data.ok) {
                            showMsg('success', data.message || 'Proveedor eliminado.');
                            row.remove();
                        } else {
                            const msg = data.message ||
                                (data.reason === 'inuse'
                                    ? 'No se pudo eliminar: hay activos asociados a este proveedor.'
                                    : 'No se pudo eliminar el proveedor.');
                            showMsg('danger', msg);
                        }
                        return;
                    }

                    // Si no vino JSON (posible redirect con HTML), recargamos filas
                    await cargar();
                } catch (e) {
                    console.error(e);
                    showMsg('danger', 'Error al eliminar.');
                }
            };
        });
    }

    function setExportLinks() {
        const params = getFilters();
        const q = qs(params);

        $btnXlsx.onclick = function () {
            const url = cfg.routes.exportXlsx + (q ? ('?' + q) : '');
            window.location.href = url;
        };

        $btnPdf.onclick = function () {
            const url = cfg.routes.exportPdf + (q ? ('?' + q) : '');
            window.location.href = url;
        };
    }

    // Eventos
    $form.addEventListener('submit', function (ev) {
        ev.preventDefault();
        setExportLinks();
        cargar();
    });

    $btnLimpiar.addEventListener('click', function () {
        resetForm();
    });

    // Búsqueda en vivo (texto/servicio/contratos)
    ['texto', 'servicio', 'noContrato', 'noProcedimiento'].forEach(name => {
        const i = $form.querySelector(`[name="${name}"]`);
        if (i) i.addEventListener('keyup', debounce(function () {
            setExportLinks();
            cargar();
        }, 400));
    });

    // Cambio de selects/fechas actualiza inmediatamente
    ['renovacionContrato', 'activo', 'venceDesde', 'venceHasta'].forEach(name => {
        const c = $form.querySelector(`[name="${name}"]`);
        if (c) c.addEventListener('change', function () {
            setExportLinks();
            cargar();
        });
    });

    // Inicial
    setExportLinks();
    wireDeleteButtons(); // por si venimos con render inicial del servidor
})();
