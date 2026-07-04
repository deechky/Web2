export class Plan {
  constructor({
    id,
    korisnikId,
    naziv,
    opis,
    pocetniDatum,
    krajnjiDatum,
    planiraniBudzet,
    napomene,
    datumKreiranja,
  }) {
    this.id = id
    this.korisnikId = korisnikId
    this.naziv = naziv
    this.opis = opis
    this.pocetniDatum = pocetniDatum
    this.krajnjiDatum = krajnjiDatum
    this.planiraniBudzet = planiraniBudzet
    this.napomene = napomene
    this.datumKreiranja = datumKreiranja
  }
}

export class Destinacija {
  constructor({ id, planId, naziv, lokacija, datumDolaska, datumOdlaska, opis }) {
    this.id = id
    this.planId = planId
    this.naziv = naziv
    this.lokacija = lokacija
    this.datumDolaska = datumDolaska
    this.datumOdlaska = datumOdlaska
    this.opis = opis
  }
}

export class Aktivnost {
  constructor({ id, planId, naziv, datum, vreme, lokacija, opis, procenjeniTrosak, status }) {
    this.id = id
    this.planId = planId
    this.naziv = naziv
    this.datum = datum
    this.vreme = vreme
    this.lokacija = lokacija
    this.opis = opis
    this.procenjeniTrosak = procenjeniTrosak
    this.status = status
  }
}

export class Trosak {
  constructor({ id, planId, naziv, kategorija, iznos, datum, opis }) {
    this.id = id
    this.planId = planId
    this.naziv = naziv
    this.kategorija = kategorija
    this.iznos = iznos
    this.datum = datum
    this.opis = opis
  }
}

export class ChecklistStavka {
  constructor({ id, planId, naziv, zavrseno }) {
    this.id = id
    this.planId = planId
    this.naziv = naziv
    this.zavrseno = zavrseno
  }
}

export class Budget {
  constructor({ planirano, potroseno, preostalo }) {
    this.planirano = planirano
    this.potroseno = potroseno
    this.preostalo = preostalo
  }
}
