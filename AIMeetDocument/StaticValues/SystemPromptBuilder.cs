using AIMeetDocument.Enums;

namespace AIMeetDocument.StaticValues;

public class SystemPromptBuilder
{
    public SystemPromptBuilder(OperationType operationType, string subject, string language, string? userPrompt,
        bool autoFilter,
        ContentDetails contentDetails, ContentStyle contentStyle)
    {
        DefaultSystemPrompt = $$"""
                                {{(string.IsNullOrWhiteSpace(userPrompt) ? "" : $"userPrompt: {userPrompt}\n")}}
                                System:
                                    {{operationType switch {
                                        OperationType.None => """
                                                              your task is to do all things that user ask you to do. Follow these rules precisely:
                                                              * **Final Output:** The entire response must be the content itself, formatted in Markdown. Do not include any introductory phrases like "Here is the content."
                                                              """,
                                        OperationType.AudioToPamphlet => $"""
                                                                          Your task is to transform a conversation transcript into an informative pamphlet. Follow these rules precisely:
                                                                          * **Final Output:** The entire response must be the pamphlet itself, formatted in Markdown. Do not include any introductory phrases like "Here is the pamphlet."
                                                                          * **Subject Focus:** The pamphlet must be exclusively about the subject: `{subject}`.
                                                                          {(autoFilter ? $"Ignore all parts of the conversation that are off-topic  and not related to subject : `{subject}`." : "")}
                                                                          * **Content Extraction:** Extract the most important points, arguments, and details from the conversation. The goal is a detailed and structured document, not a brief summary.
                                                                          * **Pamphlet Structure:** The pamphlet should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                                                          * **Content Enrichment:** If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the pamphlet's content.
                                                                          {(autoFilter ? $"* **Relevance Check:** If the conversation contains no relevant information about `{subject}`, do not generate a pamphlet. Instead, respond with ." : "")}
                                                                          """,
                                        OperationType.Pamphlet => $"""
                                                                   Your task is to create an informative pamphlet based on the provided content. Follow these rules precisely:
                                                                   * **Final Output:** The entire response must be the pamphlet itself, formatted in Markdown. Do not include any introductory phrases like "Here is the pamphlet."
                                                                   * **Subject Focus:** The pamphlet must be exclusively about the subject: `{subject}`.
                                                                   * **Content Extraction:** Extract the most important points, arguments, and details from the content. The goal is a detailed and structured document, not a brief summary.
                                                                   * **Pamphlet Structure:** The pamphlet should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                                                   * **Content Enrichment:** If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the pamphlet's content.
                                                                   """,
                                        OperationType.QuestionAnswer => $$"""
                                                                          Your task is to create perfect question and answer pairs based on the provided content. Follow these rules precisely:
                                                                          * **Final Output:** The entire response must be the question and answer pairs itself, formatted in Markdown. Do not include any introductory phrases like "Here is the question and answer pairs."
                                                                          * **Subject Focus:** The question and answer pairs must be exclusively about the subject: `{{subject}}`.
                                                                          * **Content Extraction:** Extract the most important points, arguments, and details from the content. The goal is a detailed and structured document, not a brief summary.
                                                                          * **Pamphlet Structure:** The question and answer pairs should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                                                          * **Content Enrichment:** If you see the topic need more question and answer pairs for clarity, you may add relevant question and answer pairs to enhance the content.
                                                                          """,
                                        OperationType.Remake => $$"""
                                                                  your task is to remake the provided content into an improved version. Follow these rules precisely:
                                                                  * **Final Output:** The entire response must be the remade content itself, formatted in Markdown. Do not include any introductory phrases like "Here is the remade content."
                                                                  * **Subject Focus:** The remade content must be exclusively about the subject: `{{subject}}` and provided content.
                                                                  * **Content :**The goal is a detailed and structured document, not a brief summary.
                                                                  * **Document Structure:** The remade content should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                                                  * **Content Enrichment:** If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the remade content.
                                                                  """,
                                        OperationType.Summerize => """
                                                                   your task is to summarize the provided content into a concise summary. Follow these rules precisely:
                                                                   * **Final Output:** The entire response must be the summary itself, formatted in Markdown. Do not include any introductory phrases like "Here is the summary."
                                                                   * **Subject Focus:** The summary must be exclusively about the provided content.
                                                                   * **Content Extraction:** Extract the most important points, arguments, and details from the content. The goal is a concise summary, not a detailed document.
                                                                   * **Document Structure:** The summary should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                                                   * **Content Enrichment:** If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the summary's content.
                                                                   """,
                                    }}}
                                   
                                    * **Language:** The pamphlet must be written in `{{language}}`.
                                    * **Terminology:** If the original content language is different from `{{language}}`, do not translate specific terminology, jargon, or technical terms. Keep them in the original language.
                                    {{contentDetails switch
                                    {
                                        ContentDetails.Summary => "* **Content Details:** The pamphlet should be concise, summarizing the key points discussed in the conversation.",
                                        ContentDetails.MaximumDetails => "* **Content Details:** The pamphlet should be detailed and informative, including all relevant points discussed in the conversation.",
                                        _ => ""
                                    }}}
                                    
                                    {{contentStyle switch
                                    {
                                        ContentStyle.Formal => "* **Content Style:** The pamphlet should have a formal tone, using professional language and structure.",
                                        ContentStyle.Informal => "* **Content Style:** The pamphlet should have an informal, easy to read and understand self note style .",
                                        _ => ""
                                    }}}
                                    Here is the content:
                                """;
    }

    public readonly string DefaultSystemPrompt;
}