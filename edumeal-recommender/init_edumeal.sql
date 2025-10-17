-- Drop if exists (for clean testing)
DROP TABLE IF EXISTS ingredients_allergens;
DROP TABLE IF EXISTS food_item_ingredients;
DROP TABLE IF EXISTS recipe_embeddings;
DROP TABLE IF EXISTS food_items;
DROP TABLE IF EXISTS ingredients;
DROP TABLE IF EXISTS allergens;

CREATE TABLE ingredients (
  ingredient_id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

CREATE TABLE allergens (
  allergen_id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

CREATE TABLE ingredients_allergens (
  ingredient_id INT NOT NULL REFERENCES ingredients(ingredient_id),
  allergen_id   INT NOT NULL REFERENCES allergens(allergen_id),
  PRIMARY KEY (ingredient_id, allergen_id)
);

CREATE TABLE food_items (
  food_id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  description TEXT,
  calories INT,
  popularity INT DEFAULT 0,
  created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE food_item_ingredients (
  food_id INT NOT NULL REFERENCES food_items(food_id) ON DELETE CASCADE,
  ingredient_id INT NOT NULL REFERENCES ingredients(ingredient_id),
  is_main BOOLEAN NOT NULL DEFAULT FALSE,
  PRIMARY KEY (food_id, ingredient_id)
);

CREATE TABLE recipe_embeddings (
  food_id INT PRIMARY KEY REFERENCES food_items(food_id) ON DELETE CASCADE,
  faiss_rowid INT UNIQUE NOT NULL,
  updated_at TIMESTAMP DEFAULT NOW()
);

-- docker cp init_edumeal.sql edumeal-postgres:/init.sql
