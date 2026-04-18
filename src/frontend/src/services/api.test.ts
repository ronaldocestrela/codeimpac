import api from './api'

describe('api service', () => {
  it('should configure the default baseURL', () => {
    expect(api.defaults.baseURL).toBe('http://localhost:5262/api')
  })

  it('should set json content-type header by default', () => {
    expect(api.defaults.headers['Content-Type']).toBe('application/json')
  })
})
