//var tabladata;
var tabladata;
$(function () {
    activarMenu("Mantenedor");

    // Configuración de validación del formulario
    $("#form").validate({
        rules: {
            Descripcion: {
                required: true
            }
        },
        messages: {
            Descripcion: "(*) El campo es obligatorio"
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            error.addClass('text-danger');
            error.insertAfter(element);
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
        }
    });
    // Inicializar DataTable
     tabladata = $('#tbdata').DataTable({
        ajax: {
            url: $.MisUrls.url._ObtenerProcedencia,
            type: "GET",
            dataType: "json" // 'dataType' en lugar de 'datatype'
        },
        columns: [
            { data: "Descripcion" },
            {
                data: "FechaRegistro",
                render: function (data) {
                    return formatearFecha(data);
                }
            },
            {
                data: "Activo",
                render: function (data) {
                    return data
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-danger">No Activo</span>';
                }
            },
            {
                data: "Id_Procedencia",
                render: function (data, type, row) {
                    return `
                    <button class="btn btn-primary btn-sm" type="button" onclick='abrirPopUpForm(${JSON.stringify(row)})'>
                        <i class="fas fa-pen"></i>
                    </button>
                    <button class="btn btn-danger btn-sm ms-2" type="button" onclick="eliminar(${data})">
                        <i class="fa fa-trash"></i>
                    </button>
                `;
                },
                orderable: false,
                searchable: false,
                width: "40px"
            }
        ],
        language: {
            url: $.MisUrls.url.Url_datatable_spanish
        },
        responsive: true
    });

    // Limpiar formulario al cerrar el modal
    $('#FormModal').on('hidden.bs.modal', function () {
        const $form = $('#form');
        $form[0].reset();
        $form.find('.is-valid, .is-invalid').removeClass('is-valid is-invalid');
        $form.find('span.error').remove();
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
function abrirPopUpForm(json) {
    $("#txtid").val(0);

    if (json) {
        $("#txtid").val(json.Id_Procedencia);
        $("#txtDescripcion").val(json.Descripcion);
        $("#cboEstado").val(json.Activo ? 1 : 0);
    } else {
        $("#txtDescripcion").val("");
        $("#cboEstado").val(1);
    }

    $('#FormModal').modal('show');
}

function Guardar() {
    if ($("#form").valid()) {
        var request = {
            objeto: {
                Id_Procedencia: parseInt($("#txtid").val()) || 0,
                Descripcion: $("#txtDescripcion").val(),
                Activo: parseInt($("#cboEstado").val()) === 1
            }
        };

        $.ajax({
            url: $.MisUrls.url._GuardarProcedencia,
            type: "POST",
            data: JSON.stringify(request),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.resultado) {
                    tabladata.ajax.reload();
                    $('#FormModal').modal('hide');
                    swal("Éxito", "Cambios guardados exitosamente.", "success");
                } else {
                    swal("Mensaje", "No se pudo guardar los cambios.", "warning");
                }
            },
            error: function (error) {
                console.log(error);
                swal("Error", "Ocurrió un problema al guardar.", "error");
            }
        });
    }
}
function eliminar(id) {
    swal({
        title: "Mensaje",
        text: "¿Desea eliminar la procedencia seleccionada?",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Si",
        confirmButtonColor: "#DD6B55",
        cancelButtonText: "No",
        closeOnConfirm: true
    },
        function () {
            jQuery.ajax({
                url: $.MisUrls.url._EliminarProcedencia + "?id=" + id,
                type: "GET",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.resultado) {
                        swal("Mensaje", "Procedencia eliminada correctamente.", "success");
                        tabladata.ajax.reload();
                    } else {
                        setTimeout(function () {
                            swal("Mensaje", "No se pudo eliminar la procedencia si está ligada a un caso", "warning");
                            //  swal("Mensaje", "No se pudo eliminar la procedencia. Puede estar en uso.", "warning");
                        }, 200);
                    }
                },
                error: function (error) {
                    console.error(error);
                    swal("Mensaje", "Ocurrió un error al intentar eliminar la procedencia .", "error");
                },
                beforeSend: function () {
                    // Puedes agregar alguna lógica aquí, como un indicador de carga
                },
            });
        });
}
