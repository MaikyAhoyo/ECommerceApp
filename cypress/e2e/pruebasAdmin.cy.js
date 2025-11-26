
const login = 'http://localhost:5196/auth/login'
const register = 'http://localhost:5196/auth/register'
const admin = 'http://localhost:5196/admin/'
const vendor = 'http://localhost:5196/vendor/'
const customer = 'http://localhost:5196/customer/'

const adminUser = { email: 'admin@admin.com', password: 'admin123.' }
const vendorUser = { email: 'vendor@vendor.com', password: 'vendor123.' }
const customerUser = { email: 'customer@customer.com', password: 'customer123.' }

const adminSideBar = [
  { name: 'Dashboard', path: 'dashboard' },
  { name: 'Products', path: 'products' },
  { name: 'Orders', path: 'orders' },
  { name: 'Users', path: 'users' },
  { name: 'Categories', path: 'categories' },
  { name: 'Reports', path: 'reports' },
]

function loginUser(user) {
  cy.visit(login)
  cy.get('input[id="Email"]').type(user.email)
  cy.get('input[id="Password"]').type(user.password)
  cy.get('button[type="submit"]').click({ force: true })
}

function loginAdmin() {
  loginUser(adminUser)
}

  
describe('Admin Users Management', () => {
  beforeEach(() => {
    cy.clearCookies()
    cy.clearLocalStorage()
    loginAdmin()
    cy.get('nav').contains('Users').click({ force: true })
  })

  it('should display users page', () => {
    cy.url().should('eq', admin + 'users')
  })

  it('should display users list or table', () => {
    cy.get('table').should('exist')
  })

  it('create user', () => {
    cy.get('a[href="/admin/users/create"]').should('exist')
    cy.get('a[href="/admin/users/create"]').click({ force: true })
    cy.url().should('eq', admin + 'users/create')
    cy.get('input[id="Name"]').type('Test User')
    cy.get('input[id="Email"]').type('user@user.com')
    cy.get('input[id="Password"]').type('user123.')
    cy.get('select[id="Role"]').select('Customer')
    cy.get('button[type="submit"], Add user').first().click({ force: true })
  })



  it('should search for users', () => {
    cy.get('input[type="search"], input[placeholder*="search" i]').should('exist')
  })
})

describe('Admin Categories Management', () => {
  beforeEach(() => {
    cy.clearCookies()
    cy.clearLocalStorage()
    loginAdmin()
    cy.get('nav').contains('Categories').click({ force: true })
  })

  it('should display categories page', () => {
    cy.url().should('eq', admin + 'categories')
  })

// it('should display categories list', () => {
//   cy.get('#reordenable')
//     .should('exist') 
//     .find('.group')  
//     .should('have.length.at.least', 1) 
// })


  it('should have add category button', () => {
    cy.get('a[href="/admin/categories/create"]').should('exist')

  })

  // it('create category', () => {
  //   cy.get('a[href="/admin/categories/create"]').click({ force: true })
  //   cy.url().should('eq', admin + 'categories/create')
  //   const categoryName = 'Test Category ' + Date.now()
  //   cy.get('input[id="Name"]').type(categoryName)
  //   cy.get('button[type="submit"], Add category').first().click({ force: true })
  //   cy.url().should('eq', admin + 'categories')
  //   cy.contains(categoryName).should('exist')

  // })


})

describe('Admin Reports', () => {
  beforeEach(() => {
    cy.clearCookies()
    cy.clearLocalStorage()
    loginAdmin()
    cy.get('nav').contains('Reports').click({ force: true })
  })

  it('should display reports page', () => {
    cy.url().should('eq', admin + 'reports')
  })

  it('should have report filters or options', () => {
    cy.get('select, .filter, button, [data-testid="report-filter"]').should('exist')
  })
})

describe('Admin Authorization', () => {
  beforeEach(() => {
    cy.clearCookies()
    cy.clearLocalStorage()
  })

  it('vendor should not access admin dashboard', () => {
    loginUser(vendorUser)
    cy.visit(admin + 'dashboard')
    cy.url().should('not.include', admin + 'dashboard')
  })

  it('customer should not access admin dashboard', () => {
    loginUser(customerUser)
    cy.visit(admin + 'dashboard')
    cy.url().should('not.include', admin + 'dashboard')
  })

  it('should redirect to login when not authenticated', () => {
    cy.visit(admin + 'dashboard')
    cy.url().should('include', login)
  })
})

