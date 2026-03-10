
function Guardar() {
    if ($("#form").valid()) {
        // Crear el objeto de solicitud
        var correo = $("#inputCorreo").val() || "";

        var request = {
            correo: correo,  // Enviar solo el correo, ya que ese es el parámetro esperado en el controlador
        };

        jQuery.ajax({
            url: '/Registros/EnvioCorreo',  // La ruta debe ser la correcta para tu acción en el controlador
            type: "POST",
            data: JSON.stringify(request), // Convertir el objeto a JSON
            dataType: "json", // Esperar respuesta en formato JSON
            contentType: "application/json; charset=utf-8",
            success: function (data) {
              
                if (data.resultado) {
                    swal("Éxito", "Código enviado exitosamente, revisa tu correo", "success");
                   setTimeout(function () {
                       window.location.href = "/Registros/NewContrasena?correo=" + encodeURIComponent(correo);
                    }, 3000);  
                } else {
                    swal("Mensaje", "No se pudo enviar el código. Verifica que el correo sea correcto.", "warning");
                }
            },
            error: function (xhr, status, error) {
                console.log("Estado de la solicitud:", status);
                console.log("Error:", error);
                swal("Error", "Hubo un problema al intentar enviar el código.", "error");
            }
        });
    } else {
        swal("Formulario Incompleto", "Por favor, completa todos los campos requeridos.", "warning");
    }
}
