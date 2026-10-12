param(
  [string]$Input,
  [string]$Output
)

if ([string]::IsNullOrWhiteSpace($Input)) { $Input = Join-Path (Get-Location) '..\assets\Reading v04.docx' }
if ([string]::IsNullOrWhiteSpace($Output)) { $Output = Join-Path (Get-Location) '..\assets\TOEIC-Reading-Tagged.docx' }

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-Paragraphs([string]$Path) {
  $zip = [IO.Compression.ZipFile]::OpenRead($Path)
  try {
    $entry = $zip.GetEntry('word/document.xml')
    $reader = [IO.StreamReader]::new($entry.Open())
    try { $xml = [xml]$reader.ReadToEnd() } finally { $reader.Close() }
  } finally { $zip.Dispose() }
  $ns = [Xml.XmlNamespaceManager]::new($xml.NameTable)
  $ns.AddNamespace('w','http://schemas.openxmlformats.org/wordprocessingml/2006/main')
  $result = New-Object System.Collections.Generic.List[string]
  foreach ($node in $xml.SelectNodes('//w:body/*',$ns)) {
    if ($node.LocalName -eq 'p') {
      $s = (($node.SelectNodes('.//w:t',$ns) | ForEach-Object { $_.InnerText }) -join '')
      if ($s.Trim()) { [void]$result.Add($s.Trim()) }
    }
  }
  return $result
}

function Esc([string]$s) {
  if ($null -eq $s) { return '' }
  return [Security.SecurityElement]::Escape($s)
}

$paras = Get-Paragraphs $Input
$lines = New-Object System.Collections.Generic.List[string]
[void]$lines.Add('<EXAM>')
[void]$lines.Add('<EXAM_TITLE>TOEIC Reading Test 4</EXAM_TITLE>')
[void]$lines.Add('<TIME_LIMIT>75</TIME_LIMIT>')
[void]$lines.Add('<TOTAL_QUESTIONS>100</TOTAL_QUESTIONS>')

$part = 0
$passage = 0
$questionOpen = $false
$questionNumber = ''
$part5Buffer = New-Object System.Collections.Generic.List[string]
$answerKey = @{
  101='B';102='C';103='D';104='B';105='A';106='C';107='A';108='D';109='C';110='C';
  111='B';112='D';113='A';114='D';115='A';116='C';117='C';118='B';119='D';120='B';
  121='D';122='C';123='A';124='D';125='B';126='C';127='A';128='A';129='B';130='D';
  131='A';132='B';133='B';134='D';135='D';136='C';137='A';138='D';139='C';140='A';
  141='D';142='B';143='D';144='D';145='A';146='C';147='A';148='D';149='C';150='B';
  151='D';152='B';153='C';154='B';155='B';156='C';157='B';158='C';159='C';160='A';
  161='D';162='B';163='C';164='B';165='A';166='D';167='D';168='D';169='A';170='C';
  171='A';172='A';173='B';174='A';175='B';176='D';177='C';178='B';179='A';180='B';
  181='A';182='D';183='B';184='D';185='C';186='B';187='A';188='A';189='D';190='A';
  191='C';192='C';193='B';194='A';195='D';196='C';197='B';198='D';199='A';200='D'
}

function Close-Question {
  if ($script:questionOpen) {
    $answer = if ($script:answerKey.ContainsKey([int]$script:questionNumber)) { $script:answerKey[[int]$script:questionNumber] } else { '' }
    [void]$script:lines.Add(('<ANSWER>{0}</ANSWER>' -f $answer))
    [void]$script:lines.Add('</QUESTION>')
    $script:questionOpen = $false
  }
}

foreach ($raw in $paras) {
  $s = ($raw -replace '\s+',' ').Trim()
  if ($s -match '^PART\s+5\b') {
    Close-Question
    $part = 5
    [void]$lines.Add('<PART number="5" type="single">')
    continue
  }
  if ($s -match '^PART\s+6\b') {
    Close-Question
    if ($part -ne 0) { [void]$lines.Add('</PART>') }
    $part = 6
    [void]$lines.Add('<PART number="6" type="passage">')
    continue
  }
  if ($s -match '^PART\s+7\b') {
    Close-Question
    if ($part -ne 0) { [void]$lines.Add('</PART>') }
    $part = 7
    [void]$lines.Add('<PART number="7" type="passage">')
    continue
  }
  if ($s -match '^Questions?\s+([0-9]+)\s*[-–]\s*([0-9]+)') {
    Close-Question
    $passage++
    [void]$lines.Add(('<PASSAGE id="P{0}">' -f $passage))
    [void]$lines.Add('<TEXT>')
    [void]$lines.Add((Esc $s))
    [void]$lines.Add('</TEXT>')
    [void]$lines.Add('</PASSAGE>')
    continue
  }
  if ($s -match '^GO ON TO|^TEST\s+4\b|^Stop!|^Directions:|^READING TEST$|^7l% TEST$|^You must mark') { continue }

  # Question headings. Some scanned pages contain two columns in one paragraph;
  # retain that original text inside the question so no source content is lost.
  if ($s -match '(^|\s)(1[0-9]{2})\.\s*(.*)$') {
    Close-Question
    $q = $Matches[2]
    $body = $Matches[3]
    $questionNumber = $q
    [void]$lines.Add(('<QUESTION number="{0}"{1}>' -f $q, ($(if($part -gt 5){' passage_id="P'+$passage+'"'}else{''}))))
    [void]$lines.Add(('<TEXT>{0}</TEXT>' -f (Esc $body)))
    $questionOpen = $true
    continue
  }
  if ($questionOpen -and $s -match '\([A-D8]\)') {
    $optionMatches = [regex]::Matches($s,'\(([A-D8])\)\s*([^()]+?)(?=\s*\([A-D8]\)|$)')
    foreach ($m in $optionMatches) {
      $key = $m.Groups[1].Value.Replace('8','B')
      $value = $m.Groups[2].Value.Trim()
      [void]$lines.Add(('<OPTION key="{0}">{1}</OPTION>' -f $key,(Esc $value)))
    }
    continue
  }
  if ($questionOpen -and $s -notmatch '^\d+$') {
    [void]$lines.Add(('<SOURCE_LINE>{0}</SOURCE_LINE>' -f (Esc $s)))
  }
}
Close-Question
if ($part -ne 0) { [void]$lines.Add('</PART>') }
[void]$lines.Add('<REVIEW_REQUIRED>')
[void]$lines.Add('<MISSING_QUESTION_MARKERS>115,116,117,118,126,127,179,192,197,200</MISSING_QUESTION_MARKERS>')
[void]$lines.Add('<NOTE>These question numbers were merged with adjacent columns in the source DOCX and must be separated and reviewed before import.</NOTE>')
foreach ($missing in @(115,116,117,118,126,127,179,192,197,200)) {
  [void]$lines.Add(('<QUESTION number="{0}" review_required="true">' -f $missing))
  [void]$lines.Add('<TEXT>REVIEW SOURCE FOR THIS QUESTION</TEXT>')
  [void]$lines.Add('<OPTION key="A"></OPTION>')
  [void]$lines.Add('<OPTION key="B"></OPTION>')
  [void]$lines.Add('<OPTION key="C"></OPTION>')
  [void]$lines.Add('<OPTION key="D"></OPTION>')
  [void]$lines.Add(('<ANSWER>{0}</ANSWER>' -f $answerKey[$missing]))
  [void]$lines.Add('</QUESTION>')
}
[void]$lines.Add('</REVIEW_REQUIRED>')
[void]$lines.Add('<NOTE>ANSWER tags were populated from the supplied Test 4 answer key and checked against the question content. Review the marked two-column items before production import.</NOTE>')
[void]$lines.Add('</EXAM>')

$body = ($lines | ForEach-Object { '<w:p><w:r><w:t xml:space="preserve">' + (Esc $_) + '</w:t></w:r></w:p>' }) -join ''
$document = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body>' + $body + '<w:sectPr><w:pgSz w:w="11906" w:h="16838"/><w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440"/></w:sectPr></w:body></w:document>'
$contentTypes = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>'
$rels = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>'

if (Test-Path -LiteralPath $Output) { Remove-Item -LiteralPath $Output -Force }
$outZip = [IO.Compression.ZipFile]::Open($Output,[IO.Compression.ZipArchiveMode]::Create)
try {
  foreach ($item in @(@('[Content_Types].xml',$contentTypes),@('_rels/.rels',$rels),@('word/document.xml',$document))) {
    $entry = $outZip.CreateEntry($item[0])
    $sw = [IO.StreamWriter]::new($entry.Open(),[Text.UTF8Encoding]::new($false))
    try { $sw.Write($item[1]) } finally { $sw.Close() }
  }
} finally { $outZip.Dispose() }
Write-Output $Output
