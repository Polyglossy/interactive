namespace FsAutoComplete

open FSharp.Compiler.Text
open FSharp.Compiler.Tokenization
open FSharp.Compiler.EditorServices
open FSharp.Compiler.Symbols

module KeywordList =

  // FSharp.Compiler.Service 43.13 made RichText the type of a tooltip's main description, but gave it no public
  // constructor, so the only way to turn tagged text into one for a hand-built tooltip is its internal constructor.
  // Any change to that constructor surfaces as the failure of the keyword hover tests, not as a silent regression.
  let private richTextOfTaggedText =
    let constructor =
      typeof<RichText>.GetConstructor(
        System.Reflection.BindingFlags.Instance
        ||| System.Reflection.BindingFlags.Public
        ||| System.Reflection.BindingFlags.NonPublic,
        null,
        [| typeof<TaggedText[]> |],
        null
      )

    fun (parts: TaggedText[]) -> constructor.Invoke([| box parts |]) :?> RichText

  let keywordDescriptions = FSharpKeywords.KeywordsWithDescription |> dict

  let keywordTooltips =
    keywordDescriptions
    |> Seq.map (fun kv ->
      let lines = kv.Value.Replace("\r\n", "\n").Split('\n')

      let allLines = Array.concat [| [| "<summary>" |]; lines; [| "</summary>" |] |]

      let tip =
        ToolTipText
          [ ToolTipElement.Single(
              richTextOfTaggedText [| TaggedText.tagText kv.Key |],
              FSharpXmlDoc.FromXmlText(FSharp.Compiler.Xml.XmlDoc(allLines, Range.range0))
            ) ]

      kv.Key, tip)
    |> dict

  let hashDirectives =
    [ "r", "References an assembly or a nuget: package"
      "load", "References a source .fsx script or .fs file, by compiling and running it."
      "I", "Specifies an assembly search path in quotation marks."
      "light", "Enables or disables lightweight syntax, for compatibility with other versions of ML"
      "if", "Supports conditional compilation"
      "else", "Supports conditional compilation"
      "endif", "Supports conditional compilation"
      "nowarn", "Disables a compiler warning or warnings"
      "warnon", "Enables a compiler warning or warnings"
      "quit", "exits the interactive session"
      "time", "toggles whether to display performance information"
      "line", "Indicates the original source code line" ]
    |> dict

  let allKeywords: string list =
    keywordDescriptions |> Seq.map ((|KeyValue|) >> fst) |> Seq.toList
