db = db.getSiblingDB('OfficesDb');

db.Offices.insertMany([
  { 
    Address: "Minsk, Test_Address1", 
    RegistryPhoneNumber: "+375291110011", 
    IsActive: true,
    CreatedAt: new Date()
  },
  { 
    Address: "Grodno, Test_Address2", 
    RegistryPhoneNumber: "+375294440033", 
    IsActive: true,
    CreatedAt: new Date()
  }
]);