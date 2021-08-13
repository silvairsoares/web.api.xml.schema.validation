using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using web.api.xml.schema.validation.Services.InterfacesServicos;

[Description("Validação de XML")]
public class XMLValidationController : Controller
{
    //Interface do serviço que valida o arquivo XML e será injetado automaticamente em tempo de execução
    private readonly IXMLValidationService _XMLValidationService;

    public XMLValidationController(IXMLValidationService XMLValidationService)
    {
        _XMLValidationService = XMLValidationService;
    }

    [HttpPost("api/validarxml/")]
    public string Validar([FromBody] string strDocumento)
    {
        return _XMLValidationService.XMLValidate(strDocumento);
    }

}