$(document).ready(function () 
{
    activarMenu("Seguridad");
    tabladata = $('#tbdata').DataTable({
        "ajax": {
            "url": $.MisUrls.url._ObtenerAuditoria,
            "type": "GET",
            "datatype": "json",
            "dataSrc": "data" // Asumiendo que el JSON tiene una propiedad `data` con la lista
        },
        "columns": [
            { "data": "Cod_Auditoria" },
            { "data": "Cedula_Usuario" },
            { "data": "Nom_Rol" },
            { "data": "Tabla" },
            { "data": "Procedimiento" },
            {
                "data": "ID_ColumAfectada",
                "render": function (data, type, row) {
                    return data !== null ? data : "No aplica";
                }
            },
            {
                "data": "Modificacion_JSON",
                "render": function (data, type, row) {
                    // Opcional: mostrar solo un fragmento del JSON si es muy largo
                    return data && data.length > 100 ? data.substring(0, 100) + "..." : data;
                }
            },
            {
                "data": "Fecha_Modificacion",
                "render": function (data) {
                    return formatearFecha(data); // Tu función para dar formato a la fecha
                }
            }
        ],

        "language": {
            "url": $.MisUrls.url.Url_datatable_spanish
        },
        scrollX: true,
        responsive: false,
        paging: true,
        autoWidth: false,
        dom: 'Bfrtip',
        buttons: [
            {
                extend: 'excelHtml5',
                text: 'Exportar a Excel',
                title: 'Auditoría'
            },
            'pageLength',
        ]

    });







});
function formatearFecha(fecha) {
    if (!fecha || typeof fecha !== 'string') return '';

    const match = fecha.match(/\/Date\((\d+)\)\//);
    if (!match) return '';

    const timestamp = parseInt(match[1], 10);
    const dateObj = new Date(timestamp);

    if (isNaN(dateObj)) return '';

    return dateObj.toLocaleString('es-ES', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}