import {
  extractFileNameFromContentDisposition,
  getReportExportFallbackFileName
} from './reportExport'

describe('report export utils', () => {
  it('extracts filename from standard content-disposition header', () => {
    const fileName = extractFileNameFromContentDisposition('attachment; filename="codeimpact-report-2026-04-18T10-30-00Z.pdf"')
    expect(fileName).toBe('codeimpact-report-2026-04-18T10-30-00Z.pdf')
  })

  it('extracts filename from UTF-8 encoded content-disposition header', () => {
    const fileName = extractFileNameFromContentDisposition("attachment; filename*=UTF-8''codeimpact-report-2026-04-18T10-30-00Z.docx")
    expect(fileName).toBe('codeimpact-report-2026-04-18T10-30-00Z.docx')
  })

  it('returns null when header does not include filename', () => {
    const fileName = extractFileNameFromContentDisposition('attachment')
    expect(fileName).toBeNull()
  })

  it('generates fallback filename for markdown format', () => {
    const fileName = getReportExportFallbackFileName('2026-04-18T12:30:00Z', 'markdown')
    expect(fileName.endsWith('.md')).toBe(true)
    expect(fileName).toContain('codeimpact-report-')
  })
})
