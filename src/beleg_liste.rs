use std::collections::HashMap;
use std::ops::Index;
use chrono::Datelike;
use crate::beleg::KassenBeleg;

#[derive(Debug, Default)]
pub struct KassenBelegListe {
    belege: Vec<KassenBeleg>,
}

impl KassenBelegListe {
    pub fn len(&self) -> usize {
        self.belege.len()
    }

    pub fn is_empty(&self) -> bool {
        self.belege.is_empty()
    }

    pub fn get(&self, index: usize) -> Option<&KassenBeleg> {
        self.belege.get(index)
    }

    pub fn push(&mut self, beleg: KassenBeleg) {
        self.belege.push(beleg);
    }

    pub fn pop(&mut self) -> Option<KassenBeleg> {
        self.belege.pop()
    }

    pub fn insert(&mut self, index: usize, beleg: KassenBeleg) {
        self.belege.insert(index, beleg);
    }

    pub fn remove_at(&mut self, index: usize) -> Option<KassenBeleg> {
        if index < self.belege.len() {
            Some(self.belege.remove(index))
        } else {
            None
        }
    }
    pub fn clear(&mut self) {
        self.belege.clear();
    }
    pub fn sort(&mut self) {
        self.belege.sort_by(|a, b| a.datum().cmp(&b.datum()));
    }
    pub fn swap_remove(&mut self, index: usize) -> Option<KassenBeleg> {
        if index < self.belege.len() {
            Some(self.belege.swap_remove(index))
        } else {
            None
        }
    }
    pub fn gruppiere_nach_monat(self) -> HashMap<(i32, u32), KassenBelegListe> {
        let mut map: HashMap<(i32, u32), KassenBelegListe> = HashMap::new();

        for beleg in self.belege {
            let d = beleg.datum();
            let key = (d.year(), d.month());

            map.entry(key)
                .or_insert_with(KassenBelegListe::default)
                .push(beleg);
        }

        map
    }
}

impl From<Vec<KassenBeleg>> for KassenBelegListe {
    fn from(belege: Vec<KassenBeleg>) -> Self {
        Self { belege }
    }
}

impl Index<usize> for KassenBelegListe {
    type Output = KassenBeleg;

    fn index(&self, index: usize) -> &Self::Output {
        &self.belege[index]
    }
}

impl<'a> IntoIterator for &'a KassenBelegListe {
    type Item = &'a KassenBeleg;
    type IntoIter = std::slice::Iter<'a, KassenBeleg>;

    fn into_iter(self) -> Self::IntoIter {
        self.belege.iter()
    }
}

impl IntoIterator for KassenBelegListe {
    type Item = KassenBeleg;
    type IntoIter = std::vec::IntoIter<KassenBeleg>;

    fn into_iter(self) -> Self::IntoIter {
        self.belege.into_iter()
    }
}
