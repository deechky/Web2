export class User {
  constructor({ id, ime, email, uloga }) {
    this.id = id
    this.ime = ime
    this.email = email
    this.uloga = uloga
  }

  get isAdmin() {
    return this.uloga === 'Admin'
  }
}
