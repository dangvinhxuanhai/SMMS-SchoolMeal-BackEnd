-- Ingredients
INSERT INTO ingredients(name) VALUES
 ('chicken'), ('spinach'), ('lemon'),
 ('garlic'), ('noodles'), ('peanut')
ON CONFLICT DO NOTHING;

-- Allergens
INSERT INTO allergens(name) VALUES
 ('peanut'), ('shellfish')
ON CONFLICT DO NOTHING;

-- Map ingredient→allergen (peanut ingredient → peanut allergen)
INSERT INTO ingredients_allergens(ingredient_id, allergen_id)
SELECT i.ingredient_id, a.allergen_id
FROM ingredients i, allergens a
WHERE i.name='peanut' AND a.name='peanut'
ON CONFLICT DO NOTHING;

-- Food items
INSERT INTO food_items(name, description, calories, popularity)
VALUES
 ('Grilled Lemon Chicken', 'Chicken with lemon and spinach', 420, 50),
 ('Spicy Peanut Noodles', 'Noodles with peanut sauce', 600, 80)
RETURNING food_id;

-- Link ingredients for each food
-- Grilled Lemon Chicken
INSERT INTO food_item_ingredients(food_id, ingredient_id, is_main)
SELECT f.food_id, i.ingredient_id, TRUE
FROM food_items f, ingredients i
WHERE f.name='Grilled Lemon Chicken' AND i.name='chicken'
ON CONFLICT DO NOTHING;

INSERT INTO food_item_ingredients(food_id, ingredient_id, is_main)
SELECT f.food_id, i.ingredient_id, TRUE
FROM food_items f, ingredients i
WHERE f.name='Grilled Lemon Chicken' AND i.name='spinach'
ON CONFLICT DO NOTHING;

INSERT INTO food_item_ingredients(food_id, ingredient_id, is_main)
SELECT f.food_id, i.ingredient_id, FALSE
FROM food_items f, ingredients i
WHERE f.name='Grilled Lemon Chicken' AND i.name='lemon'
ON CONFLICT DO NOTHING;

-- Spicy Peanut Noodles
INSERT INTO food_item_ingredients(food_id, ingredient_id, is_main)
SELECT f.food_id, i.ingredient_id, TRUE
FROM food_items f, ingredients i
WHERE f.name='Spicy Peanut Noodles' AND i.name='noodles'
ON CONFLICT DO NOTHING;

INSERT INTO food_item_ingredients(food_id, ingredient_id, is_main)
SELECT f.food_id, i.ingredient_id, TRUE
FROM food_items f, ingredients i
WHERE f.name='Spicy Peanut Noodles' AND i.name='peanut'
ON CONFLICT DO NOTHING;
