namespace Backend.Clients;

public static class SecopFields
{
    public const string SelectList =
        "id_del_proceso,referencia_del_proceso,entidad,nit_entidad,departamento_entidad,ciudad_entidad," +
        "nombre_del_procedimiento,descripci_n_del_procedimiento,fase,fecha_de_publicacion_del," +
        "fecha_de_ultima_publicaci,fecha_de_recepcion_de,precio_base,modalidad_de_contratacion,duracion," +
        "unidad_de_duracion,estado_del_procedimiento,adjudicado,fecha_adjudicacion,valor_total_adjudicacion," +
        "nombre_del_proveedor,nit_del_proveedor_adjudicado,codigo_principal_de_categoria," +
        "estado_de_apertura_del_proceso,tipo_de_contrato,urlproceso";

    public const string ListSelect =
        "id_del_proceso,referencia_del_proceso,entidad,departamento_entidad,ciudad_entidad," +
        "nombre_del_procedimiento,fecha_de_publicacion_del,fecha_de_recepcion_de,precio_base," +
        "modalidad_de_contratacion,estado_del_procedimiento,adjudicado,nombre_del_proveedor," +
        "tipo_de_contrato,urlproceso";

    public const string PublishedOn = "fecha_de_publicacion_del";
    public const string Status = "estado_del_procedimiento";
    public const string ProcedureName = "nombre_del_procedimiento";
    public const string Description = "descripci_n_del_procedimiento";
    public const string EntityName = "entidad";
    public const string Reference = "referencia_del_proceso";
    public const string EntityNit = "nit_entidad";
    public const string Department = "departamento_entidad";
    public const string City = "ciudad_entidad";
    public const string Modality = "modalidad_de_contratacion";
    public const string Phase = "fase";
    public const string ContractType = "tipo_de_contrato";
    public const string BasePrice = "precio_base";
    public const string ProcessId = "id_del_proceso";
    public const string Awarded = "adjudicado";
    public const string SupplierName = "nombre_del_proveedor";
}
