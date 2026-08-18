namespace Taskify.Services;

public sealed class EmailDispatchResult
{
    public int RecipientCount { get; init; }

    public int Sent { get; init; }

    public int Failed { get; init; }

    public bool SmtpConfigured { get; init; }

    public string ToastType
    {
        get
        {
            if (RecipientCount == 0 || !SmtpConfigured || Sent == 0)
            {
                return "error";
            }

            return Failed > 0 ? "warning" : "success";
        }
    }

    public string ToastMessage
    {
        get
        {
            if (RecipientCount == 0)
            {
                return "No hay destinatarios para el correo.";
            }

            if (!SmtpConfigured)
            {
                return "No se envió el correo: SMTP no está configurado.";
            }

            if (Sent == 0)
            {
                return "No se pudo enviar el correo.";
            }

            if (Failed > 0)
            {
                return $"Se enviaron {Sent} correos; {Failed} fallaron.";
            }

            return Sent == 1
                ? "Correo enviado correctamente."
                : $"Correo enviado a {Sent} destinatarios.";
        }
    }
}
