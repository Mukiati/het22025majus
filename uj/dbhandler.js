const { Sequelize, DataTypes } = require('sequelize');

const handler = new Sequelize('data', 'root', '', {
  dialect: 'mysql',
  host: 'localhost'
});

// Kategória modell
exports.categories = handler.define('category', {  // modellnév: 'category'
  id: {
    type: DataTypes.INTEGER,
    autoIncrement: true,
    allowNull: false,
    primaryKey: true
  },
  name: {
    type: DataTypes.STRING,
    allowNull: false,
    unique: true
  }
}, {
  tableName: 'categorytable'  // explicit táblanév
});

// Recept modell
exports.recipes = handler.define('recipe', {  // modellnév: 'recipe'
  id: {
    type: DataTypes.INTEGER,
    autoIncrement: true,
    allowNull: false,
    primaryKey: true
  },
  name: {
    type: DataTypes.STRING,
    allowNull: false
  },
  ingredients: {
    type: DataTypes.STRING,
    allowNull: false
  },
  categoryId: {
    type: DataTypes.INTEGER,
    allowNull: true,
    references: {
      model: 'categorytable',  // explicit táblanév
      key: 'id'
    }
  }
}, {
  tableName: 'recipetable'  // explicit táblanév
});

// Felhasználó modell
exports.users = handler.define('usertable', {
  id: {
    type: DataTypes.INTEGER,
    autoIncrement: true,
    allowNull: false,
    primaryKey: true
  },
  username: {
    type: DataTypes.STRING,
    allowNull: false
  },
  password: {
    type: DataTypes.STRING,
    allowNull: false
  }
});

// Kapcsolatok
exports.categories.hasMany(exports.recipes, { foreignKey: 'categoryId' });
exports.recipes.belongsTo(exports.categories, { 
  foreignKey: 'categoryId',
  targetKey: 'id',
  onDelete: 'SET NULL',
  onUpdate: 'CASCADE'
});

