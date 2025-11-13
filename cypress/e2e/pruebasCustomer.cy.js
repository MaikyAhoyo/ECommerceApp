const login= 'http://localhost:5196/auth/login'
const register= 'http://localhost:5196/auth/register'
const admin= 'http://localhost:5196/admin/'
const vendor= 'http://localhost:5196/vendor/'
const customer= 'http://localhost:5196/customer/'



describe('Customer login', () => {
  it('passes', () => {
    cy.visit(login)
    cy.get('input[id="Email"]').type('customer@customer.com')
    cy.get('input[id="Password"]').type('customer123.')
    cy.get('button[type="submit"]').click()
    cy.url().should('eq', customer+'home')
  })
})
