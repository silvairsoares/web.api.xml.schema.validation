using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using web.api.xml.schema.validation.Services.InterfacesServicos;

namespace web.api.xml.schema.validation.Services.Servicos
{
    public class XMLValidationService : IXMLValidationService
    {
        private static readonly ICollection<string> falhas = new List<String>();

        public string XMLValidate(string XML)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(XML);
                string falhas = ValidarXmlPessoa(document);

                if (falhas.Count() > 0)
                    return falhas;
                else
                    return "Arquivo validado com sucesso!";

            }
            catch (Exception ex)
            {
                throw new Exception("Houve um erro ao gerar um documento XML com os dados recebidos. " + ex.Message);
            }
        }

        /// <summary>
        /// Executa a validação de schema do XML do tipo "TPessoa", com base nos arquivos .xsd
        /// </summary>
        private string ValidarXmlPessoa(XmlDocument dadosNFe)
        {
            string retorno = "";
            // Inclui os shemas XSD para validação do documento do tipo "TPessoa" e suas dependências
            ICollection<string> XSDFiles = new List<String>();
            try
            {                
                XSDFiles.Add(@"Services\XMLValidation\Schemas\TPessoa.xsd");
                XSDFiles.Add(@"Services\XMLValidation\Schemas\TEndereco.xsd");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Aciona o método genérico de validações de schemas, mas que neste contexto, estará validando apenas os tipos "TEndereco" e "TPessoa"
            List<string> validacao = ValidarDocumentoXML(dadosNFe, XSDFiles).ToList();

            if (validacao.Count > 0)
            {
                retorno = "Ocorreram os seguintes erros na validação:\n";
                foreach (var item in validacao)
                {
                    retorno += item;
                }
            }
            return retorno;
        }

        /// <summary>
        /// Este é um método genérico, que serve para validar qualquer o schema de qualquer tipo de arquivo xml
        /// </summary>
        private static ICollection<string> ValidarDocumentoXML(XmlDocument doc, ICollection<string> XSDFiles)
        {
            // Limpa a lista de falhas de schema
            falhas.Clear();
            try
            {
                // Adiciona todos os arquivos .xsd ao fluxo de validação
                foreach (var item in XSDFiles)
                {
                    doc.Schemas.Add(null, item);
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("Houve um erro ao incluir os arquivos XSD para validar o arquivo XML.\n" + ex.Message);
            }
            try
            {
                // Delegate responsável por manipular os erros ocorridos: ValidationCallBack()
                doc.Validate(ValidationCallBack);
            }
            catch (XmlSchemaValidationException ex)
            {
                throw new Exception("Houve um erro executar a validação do documento XML. " + ex.Message);
            }

            return falhas;
        }

        /// <summary> 
        /// Manipulador de erros do xml
        /// Sua finalidade é obter as mensagens de erro (disparadas pelo método "ValidarDocumentoXML") e as incluir a variável "falhas"
        /// </summary> 
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            // Podem ser gerados dois tipos de falhas ("XmlSeverityType"), portanto, a estrutura abaixo, separa os erros entre "Alerta (Warning)" ou "Erros (Error)"
            if (args.Severity == XmlSeverityType.Warning)
            {
                falhas.Add("Alerta: " + TraduzMensagensDeErro(args.Message) + " (Caminho: " + ObtemCaminho(args) + ")");
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                falhas.Add("Erro: " + TraduzMensagensDeErro(args.Message) + " (Caminho: " + ObtemCaminho(args) + ")");
            }
        }

        /// <summary>
        /// Durante a validação do schema de um arquivo xml, este método auxilia na obtenção do caminho completo da tag que causou algum problema de validação
        /// </summary>
        private static string ObtemCaminho(ValidationEventArgs args)
        {
            // Captura a referência para a tag que causou o problema (falha de shema)
            XmlSchemaValidationException ex = (XmlSchemaValidationException)args.Exception;
            object sourceObject = ex.SourceObject;

            if (sourceObject.GetType() == typeof(XmlElement))
            {
                XmlElement tagProblema = (XmlElement)(sourceObject);
                return GetCaminhoTagXML(tagProblema.ParentNode) + "/" + tagProblema.Name;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Devolve o caminho completo de um elemento de um documento XML, no padrão: "\elemento_raiz\elemento2\elemento3\..."
        /// </summary>
        private static string GetCaminhoTagXML(XmlNode args)
        {
            var node = args.ParentNode;
            if (args.ParentNode == null)
            {
                return "";
            }
            else if (args.ParentNode.NodeType == XmlNodeType.Element)
            {
                // Elemento atual é um nó com mais itens
                // Chama o próprio método recursivamente, para obter toda a árvore da tag atual
                return GetCaminhoTagXML(node) + @"/" + args.Name;
            }
            return "";
        }

                /// <summary>
        /// Altera o texto das mensagens de validação do schema, de inglês para português
        /// </summary>
        private static string TraduzMensagensDeErro(string mensagem)
        {
            mensagem = mensagem.Replace("The value of the 'Algorithm' attribute does not equal its fixed value.", "O valor do atributo 'Algorithm' não é igual ao seu valor fixo.");
            mensagem = mensagem.Replace("The '", "O elemento '");
            mensagem = mensagem.Replace("element is invalid", "é inválido");
            mensagem = mensagem.Replace("The value", "O valor");
            mensagem = mensagem.Replace("is invalid according to its datatype", "é inválido de acordo com o seu tipo de dados");
            mensagem = mensagem.Replace("The Pattern constraint failed.", "");
            mensagem = mensagem.Replace("The actual length is less than the MinLength value", "O comprimento real é menor que o valor MinLength");
            mensagem = mensagem.Replace(" in namespace 'http://www.w3.org/2000/09/xmldsig#'.", "");
            mensagem = mensagem.Replace("The element", "O elemento");
            mensagem = mensagem.Replace("has invalid child element", "tem um elemento filho inválido");
            mensagem = mensagem.Replace("List of possible elements expected:", "Lista de possíveis elementos esperados:");
            mensagem = mensagem.Replace("The Enumeration constraint failed.", "");
            mensagem = mensagem.Replace("http://www.w3.org/2000/09/xmldsig#:", "");
            mensagem = mensagem.Replace("http://www.w3.org/2001/XMLSchema:", "");
            mensagem = mensagem.Replace("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.", "A entrada não é uma string Base-64 válida, pois contém um caractere não base 64, mais de dois caracteres de preenchimento ou um caractere ilegal entre os caracteres de preenchimento.");
            mensagem = mensagem.Replace("The required attribute", "O atributo obrigatório");
            mensagem = mensagem.Replace("is missing", "está ausente");
            mensagem = mensagem.Replace("has incomplete content", "tem conteúdo incompleto");
            mensagem = mensagem.Replace("as well as", "bem como");
            return mensagem;
        }
    }
}