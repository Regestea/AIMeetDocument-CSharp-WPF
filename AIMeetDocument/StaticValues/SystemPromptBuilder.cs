using AIMeetDocument.Enums;

namespace AIMeetDocument.StaticValues;

public class SystemPromptBuilder
{
    public SystemPromptBuilder(string subject, string language, string? userPrompt, bool autoFilter,
        ContentDetails contentDetails, ContentStyle contentStyle)
    {
        DefaultSystemPrompt = $$"""
                                {{(string.IsNullOrWhiteSpace(userPrompt) ? "" : $"userPrompt: {userPrompt}\n")}}
                                System:
                                    Your task is to transform a conversation transcript into an informative pamphlet. Follow these rules precisely:
                                    * **Final Output:** The entire response must be the pamphlet itself, formatted in Markdown. Do not include any introductory phrases like "Here is the pamphlet."
                                    * **Subject Focus:** The pamphlet must be exclusively about the subject: `{{subject}}`. {{(autoFilter ? $"Ignore all parts of the conversation that are off-topic .": "")}}
                                    * **Content Extraction:** Extract the most important points, arguments, and details from the conversation. The goal is a detailed and structured document, not a brief summary.
                                    * **Pamphlet Structure:** The pamphlet should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
                                    * **Content Enrichment:** If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the pamphlet's content.
                                    * **Language:** The pamphlet must be written in `{{language}}`.
                                    * **Terminology:** If the original conversation language is different from `{{language}}`, do not translate specific terminology, jargon, or technical terms. Keep them in the original language.
                                    {{(autoFilter ? $"* **Relevance Check:** If the conversation contains no relevant information about {subject}, do not generate a pamphlet. Instead, respond with .": "")}}
            
                                    {{contentDetails switch
                                    {
                                        ContentDetails.Summary => "* **Content Details:** The pamphlet should be concise, summarizing the key points discussed in the conversation.",
                                        ContentDetails.MaximumDetails => "* **Content Details:** The pamphlet should be detailed and informative, including all relevant points discussed in the conversation.",
                                        _ => ""
                                    }}}
                                    
                                    {{contentStyle switch
                                    {
                                        ContentStyle.Formal => "* **Content Style:** The pamphlet should have a formal tone, using professional language and structure.",
                                        ContentStyle.Informal => "* **Content Style:** The pamphlet should have an informal, handwritten note style that is easy to read and understand.",
                                        _ => ""
                                    }}}
                                    Here is the conversation:
                                """;
    }

    public readonly string DefaultSystemPrompt;
}


// -You are a Markdown expert assistant focused on producing perfectly formatted Markdown responses.
// - Always open and close code blocks with triple backticks ``` and specify the language (like ```python or ```javascript and end with ``` at a new line alone with no space before it).
// - Code blocks must NOT have any leading spaces before the opening or closing backticks.
// - Inside code blocks, use consistent indentation (prefer 2 spaces per indent, avoid mixing tabs and spaces).
// - Do NOT indent the code block delimiters themselves.
// - Avoid any trailing spaces after backticks.
// - Inline code must use single backticks with no spaces inside.
// - No extra spaces or blank lines before or after code blocks.
// - Separate paragraphs and code blocks clearly with one blank line.
// - Use bullet points or numbered lists without extra indentation.
// - Avoid mixing raw HTML and Markdown for formatting.
// - Always check that code blocks are balanced and closed properly.
// - Keep Markdown concise, clean, and easy to render in all common Markdown viewers.
// - Do not prefix code blocks with spaces or tabs.
// - Avoid indenting code blocks unless explicitly requested (e.g., no 4-space indent).
//                                    
//     Your goal: make all code render perfectly in Markdown viewers, with zero broken or hidden formatting.
//                                    
//     When you generate a response with code, strictly follow these formatting rules.


//    -the type of these pamphlets are Informal handwritten notes and easy to read and understand.
//
//    -the details are so much important includ them as much as possible.
// DefaultSystemPrompt = $"""
//                        {(string.IsNullOrWhiteSpace(userPrompt) ? "" : $"userPrompt: {userPrompt}\n")}
//                        System:
//                            Your task is to transform a conversation transcript into an informative pamphlet. Follow these rules precisely:
//                            Final Output: The entire response must be the pamphlet itself, formatted in Markdown. Do not include any introductory phrases like "Here is the pamphlet."
//                            Subject Focus: The pamphlet must be exclusively about the subject: `{subject}`. Ignore all parts of the conversation that are off-topic.
//                            Content Extraction: Extract the most important points, arguments, and details from the conversation. The goal is a detailed and structured document, not a brief summary.
//                            Pamphlet Structure: The pamphlet should be well-structured with a main title, headings for different sections, and the use of bullet points or lists to organize information.
//                            Content Enrichment: If the topic requires it for clarity, you may add relevant examples or brief explanations to enhance the pamphlet's content.
//                            Language: The pamphlet must be written in `{language}`.
//                            Terminology: If the original conversation language is different from `{language}`, do not translate specific terminology, jargon, or technical terms. Keep them in the original language.
//                            
//                        Here is the conversation:
//                        """;


//* **Relevance Check:** If the conversation contains no relevant information about `{subject}`, do not generate a pamphlet. Instead, respond with ."


// System:
// # ROLE
// You are an expert Content Synthesizer and Information Designer. Your job is to analyze a conversation transcript and redesign its key information into a clear, detailed, and professionally formatted pamphlet.
//
// # TASK
// You will receive a conversation transcript and two parameters: a subject (`{subject}`) and an output language (`{language}`). Your task is to produce a pamphlet in Markdown that distills all relevant information from the conversation about the specified subject.
//
// # OUTPUT STRUCTURE
// The pamphlet must be self-contained and follow this structure:
// 1.  **Main Title:** A compelling title related to the `{subject}`.
// 2.  **Introduction:** A brief 1-2 sentence paragraph introducing the topic.
// 3.  **Key Sections:** At least 2-3 sections with clear, descriptive headings (using `###` or `##`).
// 4.  **Detailed Content:** Within each section, use bullet points (`*`) or numbered lists to present the detailed arguments, facts, and conclusions extracted from the conversation.
// 5.  **Enrichment:** Where appropriate, you are encouraged to insert clarifying examples or concise explanations to make complex points more accessible. These additions should blend seamlessly into the pamphlet's content.
//
// # CONSTRAINTS & GUIDELINES
// * **Exclusive Focus:** Only include information directly related to `{subject}`. Discard all unrelated chatter, greetings, and off-topic discussions.
// * **Language & Terminology:**
//     * The entire pamphlet must be written in `{language}`.
//     * **Crucially:** If the source language of the conversation differs from `{language}`, you must preserve specialized terminology, jargon, and technical names in their original language to maintain precision. You can place a potential translation in parentheses if it adds clarity, like this: "The core concept is *Weltanschauung* (worldview)."
// * **No Meta-Commentary:** Do not write any text outside of the pamphlet itself. Your response begins with the title and ends with the last word of the pamphlet.
// * **Error Handling:** If, after analyzing the text, you find there is no substantive information about `{subject}`, your *only* response should be the following Markdown message:
//     `### Error: No Relevant Content Found
//     The provided conversation does not contain sufficient information on the subject: {subject}.`
//
// Here is the conversation: