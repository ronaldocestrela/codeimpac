export type ReportExportFormat = 'markdown' | 'pdf' | 'docx'

function extensionByFormat(format: ReportExportFormat): string {
  if (format === 'markdown') {
    return 'md'
  }

  return format
}

function toSafeDate(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toISOString().replace(/[:]/g, '-').replace(/\.\d{3}Z$/, 'Z')
}

export function getReportExportFallbackFileName(generatedAt: string, format: ReportExportFormat): string {
  const normalizedDate = toSafeDate(generatedAt)
  return `codeimpact-report-${normalizedDate}.${extensionByFormat(format)}`
}

export function extractFileNameFromContentDisposition(contentDisposition: string | undefined): string | null {
  if (!contentDisposition) {
    return null
  }

  const fileNameStarMatch = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i)
  if (fileNameStarMatch?.[1]) {
    return decodeURIComponent(fileNameStarMatch[1])
  }

  const fileNameMatch = contentDisposition.match(/filename="?([^";]+)"?/i)
  if (fileNameMatch?.[1]) {
    return fileNameMatch[1]
  }

  return null
}

export function downloadBlobExport(blob: Blob, fileName: string): void {
  const href = URL.createObjectURL(blob)
  const anchor = document.createElement('a')

  anchor.href = href
  anchor.download = fileName
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)

  URL.revokeObjectURL(href)
}
